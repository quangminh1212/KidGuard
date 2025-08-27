using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ChildGuard.Core.Events
{
    /// <summary>
    /// Implementation của Event Dispatcher - thread-safe
    /// </summary>
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers;
        private readonly ReaderWriterLockSlim _lock;
        private readonly bool _throwOnError;
        
        public EventDispatcher(bool throwOnError = false)
        {
            _handlers = new ConcurrentDictionary<Type, List<Delegate>>();
            _lock = new ReaderWriterLockSlim();
            _throwOnError = throwOnError;
        }
        
        /// <summary>
        /// Đăng ký handler đồng bộ
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }
                
                _handlers[eventType].Add(handler);
                Debug.WriteLine($"[EventDispatcher] Subscribed handler for {eventType.Name}. Total handlers: {_handlers[eventType].Count}");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Đăng ký handler bất đồng bộ
        /// </summary>
        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }
                
                _handlers[eventType].Add(handler);
                Debug.WriteLine($"[EventDispatcher] Subscribed async handler for {eventType.Name}. Total handlers: {_handlers[eventType].Count}");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Hủy đăng ký handler đồng bộ
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType].Remove(handler);
                    if (_handlers[eventType].Count == 0)
                    {
                        _handlers.TryRemove(eventType, out _);
                    }
                    Debug.WriteLine($"[EventDispatcher] Unsubscribed handler for {eventType.Name}. Remaining handlers: {_handlers[eventType]?.Count ?? 0}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Hủy đăng ký handler bất đồng bộ
        /// </summary>
        public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType].Remove(handler);
                    if (_handlers[eventType].Count == 0)
                    {
                        _handlers.TryRemove(eventType, out _);
                    }
                    Debug.WriteLine($"[EventDispatcher] Unsubscribed async handler for {eventType.Name}. Remaining handlers: {_handlers[eventType]?.Count ?? 0}");
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Phát sự kiện đồng bộ
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));
            
            List<Delegate> handlersToInvoke;
            
            _lock.EnterReadLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType) || _handlers[eventType].Count == 0)
                {
                    Debug.WriteLine($"[EventDispatcher] No handlers registered for {eventType.Name}");
                    return;
                }
                
                // Copy handlers để tránh lock khi invoke
                handlersToInvoke = new List<Delegate>(_handlers[eventType]);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            
            Debug.WriteLine($"[EventDispatcher] Publishing {typeof(TEvent).Name} to {handlersToInvoke.Count} handlers");
            
            var exceptions = new List<Exception>();
            
            foreach (var handler in handlersToInvoke)
            {
                try
                {
                    switch (handler)
                    {
                        case Action<TEvent> action:
                            action(eventData);
                            break;
                        case Func<TEvent, Task> func:
                            // Chạy async handler đồng bộ
                            func(eventData).GetAwaiter().GetResult();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EventDispatcher] Error in handler: {ex.Message}");
                    exceptions.Add(ex);
                    
                    if (_throwOnError)
                        throw;
                }
            }
            
            if (exceptions.Any() && _throwOnError)
            {
                throw new AggregateException("One or more handlers threw exceptions", exceptions);
            }
        }
        
        /// <summary>
        /// Phát sự kiện bất đồng bộ
        /// </summary>
        public async Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IEvent
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));
            
            List<Delegate> handlersToInvoke;
            
            _lock.EnterReadLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType) || _handlers[eventType].Count == 0)
                {
                    Debug.WriteLine($"[EventDispatcher] No handlers registered for {eventType.Name}");
                    return;
                }
                
                // Copy handlers để tránh lock khi invoke
                handlersToInvoke = new List<Delegate>(_handlers[eventType]);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            
            Debug.WriteLine($"[EventDispatcher] Publishing async {typeof(TEvent).Name} to {handlersToInvoke.Count} handlers");
            
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();
            
            foreach (var handler in handlersToInvoke)
            {
                Task task = null!;
                
                try
                {
                    switch (handler)
                    {
                        case Action<TEvent> action:
                            task = Task.Run(() => action(eventData));
                            break;
                        case Func<TEvent, Task> func:
                            task = func(eventData);
                            break;
                    }
                    
                    if (task != null)
                        tasks.Add(task);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EventDispatcher] Error starting handler: {ex.Message}");
                    exceptions.Add(ex);
                    
                    if (_throwOnError)
                        throw;
                }
            }
            
            if (tasks.Any())
            {
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EventDispatcher] Error in async handlers: {ex.Message}");
                    exceptions.Add(ex);
                    
                    if (_throwOnError)
                        throw;
                }
            }
            
            if (exceptions.Any() && _throwOnError)
            {
                throw new AggregateException("One or more handlers threw exceptions", exceptions);
            }
        }
        
        /// <summary>
        /// Xóa tất cả handlers
        /// </summary>
        public void ClearAllHandlers()
        {
            _lock.EnterWriteLock();
            try
            {
                _handlers.Clear();
                Debug.WriteLine("[EventDispatcher] All handlers cleared");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
        /// <summary>
        /// Lấy số lượng handlers cho một loại event
        /// </summary>
        public int GetHandlerCount<TEvent>() where TEvent : IEvent
        {
            _lock.EnterReadLock();
            try
            {
                var eventType = typeof(TEvent);
                return _handlers.ContainsKey(eventType) ? _handlers[eventType].Count : 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            ClearAllHandlers();
            _lock?.Dispose();
        }
    }
}

using System;
using System.Threading.Tasks;

namespace ChildGuard.Core.Events
{
    /// <summary>
    /// Interface cho Event Dispatcher - trung tâm xử lý sự kiện
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Đăng ký handler cho một loại sự kiện
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Đăng ký async handler cho một loại sự kiện
        /// </summary>
        void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Hủy đăng ký handler
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Hủy đăng ký async handler
        /// </summary>
        void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IEvent;
        
        /// <summary>
        /// Phát sự kiện đồng bộ
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : IEvent;
        
        /// <summary>
        /// Phát sự kiện bất đồng bộ
        /// </summary>
        Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IEvent;
        
        /// <summary>
        /// Xóa tất cả handlers
        /// </summary>
        void ClearAllHandlers();
        
        /// <summary>
        /// Lấy số lượng handlers đã đăng ký cho một loại event
        /// </summary>
        int GetHandlerCount<TEvent>() where TEvent : IEvent;
    }
    
    /// <summary>
    /// Interface cơ bản cho tất cả events
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// ID duy nhất của event
        /// </summary>
        Guid EventId { get; }
        
        /// <summary>
        /// Thời điểm phát sinh event
        /// </summary>
        DateTime Timestamp { get; }
        
        /// <summary>
        /// Nguồn phát sinh event
        /// </summary>
        string Source { get; }
    }
    
    /// <summary>
    /// Interface cho event source - nguồn phát sự kiện
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// Tên của event source
        /// </summary>
        string SourceName { get; }
        
        /// <summary>
        /// Dispatcher để phát sự kiện
        /// </summary>
        IEventDispatcher? Dispatcher { get; set; }
    }
    
    /// <summary>
    /// Interface cho event sink - nơi nhận sự kiện
    /// </summary>
    public interface IEventSink
    {
        /// <summary>
        /// Xử lý sự kiện
        /// </summary>
        Task HandleEventAsync(IEvent eventData);
        
        /// <summary>
        /// Kiểm tra có thể xử lý loại event này không
        /// </summary>
        bool CanHandle(Type eventType);
    }
}

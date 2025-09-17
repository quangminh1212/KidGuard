using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using KidGuard.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace KidGuard.Forms
{
    /// <summary>
    /// Form đăng nhập cho ứng dụng KidGuard
    /// Xử lý xác thực người dùng và thiết lập mật khẩu lần đầu
    /// </summary>
    public partial class LoginForm : Form
    {
        // Service xác thực
        private readonly IAuthenticationService _authService;
        
        // Controls cho form đăng nhập
        private TextBox txtPassword;
        private TextBox txtEmail;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnLogin;
        private Button btnSetup;
        private Label lblTitle;
        private Label lblMessage;
        private Panel pnlLogin;
        private Panel pnlSetup;
        private CheckBox chkShowPassword;
        private LinkLabel lnkForgotPassword;
        
        // Biến trạng thái
        private bool _isFirstRun;
        private int _loginAttempts = 0;
        private const int MaxLoginAttempts = 3;

        /// <summary>
        /// Constructor của LoginForm
        /// </summary>
        public LoginForm(IAuthenticationService authService)
        {
            _authService = authService;
            InitializeComponent();
            _ = CheckFirstRunAsync();
        }

        /// <summary>
        /// Khởi tạo giao diện form
        /// </summary>
        private void InitializeComponent()
        {
            // Cấu hình form chính
            Text = "KidGuard - Đăng Nhập";
            Size = new Size(450, 350);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Icon = SystemIcons.Shield;
            BackColor = Color.FromArgb(240, 240, 240);

            // Tiêu đề
            lblTitle = new Label
            {
                Text = "KidGuard",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(0, 20),
                Size = new Size(450, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Label thông báo
            lblMessage = new Label
            {
                Text = "Bảo vệ con bạn trên không gian mạng",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(0, 70),
                Size = new Size(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Panel đăng nhập
            pnlLogin = new Panel
            {
                Location = new Point(75, 110),
                Size = new Size(300, 150),
                Visible = false
            };

            // Password textbox cho đăng nhập
            var lblPassword = new Label
            {
                Text = "Mật khẩu:",
                Location = new Point(0, 10),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10)
            };

            txtPassword = new TextBox
            {
                Location = new Point(0, 35),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true
            };
            txtPassword.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Enter) btnLogin.PerformClick();
            };

            // Checkbox hiển thị mật khẩu
            chkShowPassword = new CheckBox
            {
                Text = "Hiện mật khẩu",
                Location = new Point(0, 70),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
            {
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                if (txtNewPassword != null)
                {
                    txtNewPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                    txtConfirmPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
                }
            };

            // Link quên mật khẩu
            lnkForgotPassword = new LinkLabel
            {
                Text = "Quên mật khẩu?",
                Location = new Point(200, 70),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleRight
            };
            lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;

            // Nút đăng nhập
            btnLogin = new Button
            {
                Text = "Đăng Nhập",
                Location = new Point(0, 100),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.Click += BtnLogin_Click;
            btnLogin.FlatAppearance.BorderSize = 0;

            pnlLogin.Controls.AddRange(new Control[] {
                lblPassword, txtPassword, chkShowPassword, lnkForgotPassword, btnLogin
            });

            // Panel thiết lập mật khẩu lần đầu
            pnlSetup = new Panel
            {
                Location = new Point(75, 100),
                Size = new Size(300, 200),
                Visible = false
            };

            // Email textbox
            var lblEmail = new Label
            {
                Text = "Email (tùy chọn):",
                Location = new Point(0, 0),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9)
            };

            txtEmail = new TextBox
            {
                Location = new Point(0, 20),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "email@example.com"
            };

            // Mật khẩu mới
            var lblNewPassword = new Label
            {
                Text = "Mật khẩu mới:",
                Location = new Point(0, 50),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9)
            };

            txtNewPassword = new TextBox
            {
                Location = new Point(0, 70),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                UseSystemPasswordChar = true
            };

            // Xác nhận mật khẩu
            var lblConfirmPassword = new Label
            {
                Text = "Xác nhận mật khẩu:",
                Location = new Point(0, 100),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9)
            };

            txtConfirmPassword = new TextBox
            {
                Location = new Point(0, 120),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                UseSystemPasswordChar = true
            };

            // Nút thiết lập
            btnSetup = new Button
            {
                Text = "Thiết Lập Mật Khẩu",
                Location = new Point(0, 155),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSetup.Click += BtnSetup_Click;
            btnSetup.FlatAppearance.BorderSize = 0;

            pnlSetup.Controls.AddRange(new Control[] {
                lblEmail, txtEmail, lblNewPassword, txtNewPassword,
                lblConfirmPassword, txtConfirmPassword, btnSetup
            });

            // Thêm controls vào form
            Controls.AddRange(new Control[] { lblTitle, lblMessage, pnlLogin, pnlSetup });
        }

        /// <summary>
        /// Kiểm tra xem có phải lần chạy đầu tiên không
        /// </summary>
        private async Task CheckFirstRunAsync()
        {
            try
            {
                _isFirstRun = await _authService.IsFirstRunAsync();
                
                if (_isFirstRun)
                {
                    // Hiển thị form thiết lập mật khẩu
                    lblMessage.Text = "Chào mừng! Vui lòng thiết lập mật khẩu cho lần đầu sử dụng";
                    lblMessage.ForeColor = Color.Green;
                    pnlSetup.Visible = true;
                    pnlLogin.Visible = false;
                }
                else
                {
                    // Hiển thị form đăng nhập
                    pnlLogin.Visible = true;
                    pnlSetup.Visible = false;
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện đăng nhập
        /// </summary>
        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Đang xác thực...";

                var success = await _authService.LoginAsync(txtPassword.Text);
                
                if (success)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _loginAttempts++;
                    
                    if (_loginAttempts >= MaxLoginAttempts)
                    {
                        MessageBox.Show(
                            "Bạn đã nhập sai mật khẩu quá nhiều lần!\n" +
                            "Ứng dụng sẽ bị khóa trong 5 phút.",
                            "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        // Khóa form trong 5 phút
                        btnLogin.Enabled = false;
                        var timer = new System.Windows.Forms.Timer { Interval = 300000 }; // 5 phút
                        timer.Tick += (s, args) =>
                        {
                            btnLogin.Enabled = true;
                            _loginAttempts = 0;
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                    else
                    {
                        int remainingAttempts = MaxLoginAttempts - _loginAttempts;
                        MessageBox.Show(
                            $"Mật khẩu không đúng!\nCòn {remainingAttempts} lần thử.",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btnLogin.Enabled)
                {
                    btnLogin.Text = "Đăng Nhập";
                }
            }
        }

        /// <summary>
        /// Xử lý thiết lập mật khẩu lần đầu
        /// </summary>
        private async void BtnSetup_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPassword.Text.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSetup.Enabled = false;
                btnSetup.Text = "Đang thiết lập...";

                var success = await _authService.SetupInitialPasswordAsync(
                    txtNewPassword.Text, txtEmail.Text);
                
                if (success)
                {
                    MessageBox.Show(
                        "Thiết lập mật khẩu thành công!\n" +
                        "Vui lòng ghi nhớ mật khẩu của bạn.",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Không thể thiết lập mật khẩu!", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSetup.Enabled = true;
                btnSetup.Text = "Thiết Lập Mật Khẩu";
            }
        }

        /// <summary>
        /// Xử lý quên mật khẩu
        /// </summary>
        private async void LnkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            var email = Microsoft.VisualBasic.Interaction.InputBox(
                "Nhập email đã đăng ký để nhận mã khôi phục:",
                "Quên mật khẩu", "");
            
            if (string.IsNullOrWhiteSpace(email))
                return;

            try
            {
                var success = await _authService.SendPasswordResetCodeAsync(email);
                
                if (success)
                {
                    MessageBox.Show(
                        "Mã khôi phục đã được gửi đến email của bạn!\n" +
                        "Vui lòng kiểm tra email (hoặc xem trong log).",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Nhập mã reset
                    var resetCode = Microsoft.VisualBasic.Interaction.InputBox(
                        "Nhập mã khôi phục 6 số:", "Khôi phục mật khẩu", "");
                    
                    if (string.IsNullOrWhiteSpace(resetCode))
                        return;
                    
                    // Nhập mật khẩu mới
                    var newPassword = Microsoft.VisualBasic.Interaction.InputBox(
                        "Nhập mật khẩu mới:", "Khôi phục mật khẩu", "");
                    
                    if (string.IsNullOrWhiteSpace(newPassword))
                        return;
                    
                    var resetSuccess = await _authService.ResetPasswordAsync(resetCode, newPassword);
                    
                    if (resetSuccess)
                    {
                        MessageBox.Show("Đã đặt lại mật khẩu thành công!", "Thành công", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Mã khôi phục không đúng hoặc đã hết hạn!", "Lỗi", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Email không tồn tại trong hệ thống!", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xử lý khi form đóng
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                var result = MessageBox.Show(
                    "Bạn cần đăng nhập để sử dụng KidGuard.\nBạn có muốn thoát?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            
            base.OnFormClosing(e);
        }
    }
}
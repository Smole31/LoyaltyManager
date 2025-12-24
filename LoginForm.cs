using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoyaltyManager
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Label lblMessage = null!;
        public User? LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.lblMessage = new Label();

            this.Text = "Вхід в систему";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(236, 240, 241);

            var lblTitle = new Label
            {
                Text = "Система лояльності",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(90, 30),
                AutoSize = true,
                ForeColor = Color.FromArgb(41, 128, 185)
            };

            var lblUsername = new Label
            {
                Text = "Логін:",
                Location = new Point(50, 90),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtUsername.Location = new Point(50, 115);
            txtUsername.Size = new Size(280, 30);
            txtUsername.Font = new Font("Segoe UI", 11);

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 155),
                Size = new Size(280, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtPassword.Location = new Point(50, 180);
            txtPassword.Size = new Size(280, 30);
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.PasswordChar = '*';

            lblMessage.Location = new Point(50, 220);
            lblMessage.Size = new Size(280, 20);
            lblMessage.ForeColor = Color.Red;
            lblMessage.Font = new Font("Segoe UI", 9);
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;

            var btnLogin = new Button
            {
                Text = "Увійти",
                Location = new Point(50, 250),
                Size = new Size(280, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            var lblHint = new Label
            {
                Text = "Логін: admin, пароль: 1234",
                Location = new Point(50, 300),
                Size = new Size(280, 20),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername,
                lblPassword, txtPassword, lblMessage,
                btnLogin, lblHint
            });

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Заповніть всі поля!";
                return;
            }

            LoggedInUser = DatabaseHelper.Login(username, password);

            if (LoggedInUser != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                lblMessage.Text = "Невірний логін або пароль!";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
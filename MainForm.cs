using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace LoyaltyManager
{
    public partial class MainForm : Form
    {
        private User currentUser = null!;
        private Label lblUserName = null!;
        private Label lblUserPoints = null!;
        private DataGridView dgvProducts = null!;
        private DataGridView dgvPurchases = null!;

        public MainForm(User? user)
        {
            if (user == null)
            {
                MessageBox.Show("Помилка автентифікації");
                Application.Exit();
                return;
            }

            currentUser = user;
            InitializeComponent();
            InitializeComponent1();
            LoadData();
        }

        private void InitializeComponent1()
        {
            this.Text = "Система лояльності";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(236, 240, 241);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            lblUserName = new Label
            {
                Text = $"Користувач: {currentUser.FullName}",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White
            };

            lblUserPoints = new Label
            {
                Text = $"Бали: {currentUser.Points}",
                Location = new Point(20, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White
            };

            var btnLogout = new Button
            {
                Text = "Вийти",
                Location = new Point(850, 20),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => {
                this.Close();
                Application.Restart();
            };

            topPanel.Controls.AddRange(new Control[] { lblUserName, lblUserPoints, btnLogout });

            var lblProducts = new Label
            {
                Text = "Доступні продукти",
                Location = new Point(20, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            dgvProducts = new DataGridView
            {
                Location = new Point(20, 140),
                Size = new Size(940, 250),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            var btnBuy = new Button
            {
                Text = "Купити товар",
                Location = new Point(20, 400),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBuy.FlatAppearance.BorderSize = 0;
            btnBuy.Click += BtnBuy_Click;

            var lblHistory = new Label
            {
                Text = "Історія покупок",
                Location = new Point(20, 460),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            dgvPurchases = new DataGridView
            {
                Location = new Point(20, 500),
                Size = new Size(940, 140),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            this.Controls.AddRange(new Control[] {
                topPanel, lblProducts, dgvProducts, btnBuy,
                lblHistory, dgvPurchases
            });
        }

        private void LoadData()
        {
            LoadProducts();
            LoadPurchases();
            UpdateUserPoints();
        }

        private void LoadProducts()
        {
            using (var connection = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        product_id AS 'ID',
                        product_name AS 'Назва',
                        price AS 'Ціна',
                        points_per_unit AS 'Бали',
                        stock_quantity AS 'Залишок'
                    FROM products
                    ORDER BY product_name";

                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    dgvProducts.DataSource = dt;
                    if (dgvProducts.Columns.Count > 0)
                        dgvProducts.Columns["ID"].Visible = false;
                }
            }
        }

        private void LoadPurchases()
        {
            using (var connection = new SqliteConnection(DatabaseHelper.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        p.product_name AS 'Товар',
                        pu.quantity AS 'Кількість',
                        pu.total_price AS 'Сума',
                        pu.points_earned AS 'Бали',
                        datetime(pu.purchase_date) AS 'Дата'
                    FROM purchases pu
                    JOIN products p ON pu.product_id = p.product_id
                    WHERE pu.user_id = @userId
                    ORDER BY pu.purchase_date DESC
                    LIMIT 10";
                cmd.Parameters.AddWithValue("@userId", currentUser.UserId);

                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    dgvPurchases.DataSource = dt;
                }
            }
        }

        private void UpdateUserPoints()
        {
            currentUser.Points = DatabaseHelper.GetUserPoints(currentUser.UserId);
            lblUserPoints.Text = $"Бали: {currentUser.Points}";
        }

        private void BtnBuy_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть товар!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvProducts.SelectedRows[0];
            int productId = Convert.ToInt32(row.Cells["ID"].Value);
            string productName = row.Cells["Назва"].Value?.ToString() ?? "";
            decimal price = Convert.ToDecimal(row.Cells["Ціна"].Value);
            int pointsPerUnit = Convert.ToInt32(row.Cells["Бали"].Value);

            string input = PromptForQuantity();
            if (string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введіть число!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal totalPrice = price * quantity;
            int pointsEarned = pointsPerUnit * quantity;

            var result = MessageBox.Show(
                $"Товар: {productName}\nКількість: {quantity}\nСума: {totalPrice:F2} грн\nБали: +{pointsEarned}\n\nПідтвердити?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                MakePurchase(productId, quantity, totalPrice, pointsEarned);
            }
        }

        private string PromptForQuantity()
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Кількість",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Скільки купити?", AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240, Text = "1" };
            Button confirmation = new Button() { Text = "OK", Left = 100, Width = 80, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void MakePurchase(int productId, int quantity, decimal totalPrice, int pointsEarned)
        {
            try
            {
                using (var connection = new SqliteConnection(DatabaseHelper.ConnectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO purchases (user_id, product_id, quantity, total_price, points_earned)
                        VALUES (@userId, @productId, @quantity, @totalPrice, @points)";
                    cmd.Parameters.AddWithValue("@userId", currentUser.UserId);
                    cmd.Parameters.AddWithValue("@productId", productId);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@totalPrice", totalPrice);
                    cmd.Parameters.AddWithValue("@points", pointsEarned);
                    cmd.ExecuteNonQuery();

                    DatabaseHelper.AddPoints(currentUser.UserId, pointsEarned);
                }

                MessageBox.Show($"Успіх! Нараховано {pointsEarned} балів!", "Успіх",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message, "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
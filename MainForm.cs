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
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.MinimumSize = new Size(1000, 700);

            // Верхня панель
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            // Іконка користувача
            var userIcon = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI", 28),
                Location = new Point(20, 25),
                Size = new Size(50, 50),
                ForeColor = Color.White
            };

            lblUserName = new Label
            {
                Text = currentUser.FullName,
                Location = new Point(80, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White
            };

            lblUserPoints = new Label
            {
                Text = $"⭐ {currentUser.Points} балів",
                Location = new Point(80, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(255, 235, 59)
            };

            var btnLogout = new Button
            {
                Text = "🚪 Вийти",
                Location = new Point(1050, 30),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
            btnLogout.Click += (s, e) => {
                this.Close();
                Application.Restart();
            };

            topPanel.Controls.AddRange(new Control[] { userIcon, lblUserName, lblUserPoints, btnLogout });

            // Панель продуктів
            var productsPanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(1150, 320),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblProducts = new Label
            {
                Text = "🛒 Доступні продукти",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            dgvProducts = new DataGridView
            {
                Location = new Point(15, 50),
                Size = new Size(1120, 210),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                MultiSelect = false
            };

            // Стилі для заголовків
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvProducts.ColumnHeadersHeight = 40;

            // Стилі для рядків
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvProducts.DefaultCellStyle.Padding = new Padding(5);
            dgvProducts.RowTemplate.Height = 35;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);

            var btnBuy = new Button
            {
                Text = "💳 КУПИТИ ОБРАНИЙ ТОВАР",
                Location = new Point(15, 270),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBuy.FlatAppearance.BorderSize = 0;
            btnBuy.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            btnBuy.Click += BtnBuy_Click;

            productsPanel.Controls.AddRange(new Control[] { lblProducts, dgvProducts, btnBuy });

            // Панель історії
            var historyPanel = new Panel
            {
                Location = new Point(20, 460),
                Size = new Size(1150, 280),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblHistory = new Label
            {
                Text = "📋 Історія покупок",
                Location = new Point(15, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            dgvPurchases = new DataGridView
            {
                Location = new Point(15, 50),
                Size = new Size(1120, 215),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            dgvPurchases.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(155, 89, 182);
            dgvPurchases.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPurchases.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvPurchases.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvPurchases.ColumnHeadersHeight = 40;

            dgvPurchases.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvPurchases.DefaultCellStyle.Padding = new Padding(5);
            dgvPurchases.RowTemplate.Height = 35;
            dgvPurchases.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);

            historyPanel.Controls.AddRange(new Control[] { lblHistory, dgvPurchases });

            this.Controls.AddRange(new Control[] { topPanel, productsPanel, historyPanel });
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
                        product_name AS '📦 Назва',
                        printf('%.2f', price) AS '💰 Ціна (грн)',
                        points_per_unit AS '⭐ Бали',
                        stock_quantity AS '📊 Залишок'
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
                        p.product_name AS '📦 Товар',
                        pu.quantity AS '🔢 Кількість',
                        printf('%.2f', pu.total_price) AS '💰 Сума (грн)',
                        pu.points_earned AS '⭐ Бали',
                        strftime('%d.%m.%Y %H:%M', pu.purchase_date) AS '📅 Дата'
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
            lblUserPoints.Text = $"⭐ {currentUser.Points} балів";
        }

        private void BtnBuy_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("❌ Оберіть товар для покупки!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvProducts.SelectedRows[0];
            int productId = Convert.ToInt32(row.Cells["ID"].Value);
            string productName = row.Cells["📦 Назва"].Value?.ToString() ?? "";
            string priceStr = row.Cells["💰 Ціна (грн)"].Value?.ToString() ?? "0";
            decimal price = decimal.Parse(priceStr);
            int pointsPerUnit = Convert.ToInt32(row.Cells["⭐ Бали"].Value);

            string input = PromptForQuantity();
            if (string.IsNullOrEmpty(input)) return;

            if (!int.TryParse(input, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("❌ Введіть коректну кількість!", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal totalPrice = price * quantity;
            int pointsEarned = pointsPerUnit * quantity;

            var result = MessageBox.Show(
                $"🛒 Товар: {productName}\n" +
                $"🔢 Кількість: {quantity}\n" +
                $"💰 Сума: {totalPrice:F2} грн\n" +
                $"⭐ Бали: +{pointsEarned}\n\n" +
                $"✅ Підтвердити покупку?",
                "Підтвердження покупки",
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
                Width = 350,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Кількість товару",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label textLabel = new Label()
            {
                Left = 30,
                Top = 30,
                Text = "🔢 Скільки одиниць купити?",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            TextBox textBox = new TextBox()
            {
                Left = 30,
                Top = 65,
                Width = 280,
                Text = "1",
                Font = new Font("Segoe UI", 14),
                TextAlign = HorizontalAlignment.Center
            };

            Button confirmation = new Button()
            {
                Text = "✅ OK",
                Left = 80,
                Width = 180,
                Top = 105,
                Height = 40,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            confirmation.FlatAppearance.BorderSize = 0;

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

                MessageBox.Show(
                    $"🎉 Покупка успішна!\n\n" +
                    $"⭐ Нараховано {pointsEarned} балів!\n" +
                    $"💰 Загальна сума: {totalPrice:F2} грн",
                    "✅ Успіх",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Помилка: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
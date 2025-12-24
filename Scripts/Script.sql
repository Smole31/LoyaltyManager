-- ========================================
-- БАЗА ДАНИХ МОДУЛЯ ЛОЯЛЬНОСТІ
-- Продуктовий магазин (SQLite)
-- ========================================

-- Таблиця категорій продуктів
CREATE TABLE category (
    category_id INTEGER PRIMARY KEY AUTOINCREMENT,
    category_name TEXT NOT NULL,
    description TEXT,
    parent_category_id INTEGER,
    is_active INTEGER DEFAULT 1,
    FOREIGN KEY (parent_category_id) REFERENCES category(category_id)
);

CREATE INDEX idx_category_parent ON category(parent_category_id);
CREATE INDEX idx_category_active ON category(is_active);

-- Таблиця продуктів
CREATE TABLE products (
    product_id INTEGER PRIMARY KEY AUTOINCREMENT,
    product_name TEXT NOT NULL,
    category_id INTEGER NOT NULL,
    barcode TEXT UNIQUE,
    price REAL NOT NULL,
    bonus_points_multiplier REAL DEFAULT 1.00,
    stock_quantity REAL DEFAULT 0,
    unit TEXT DEFAULT 'шт',
    is_active INTEGER DEFAULT 1,
    description TEXT,
    image_url TEXT,
    FOREIGN KEY (category_id) REFERENCES category(category_id)
);

CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_barcode ON products(barcode);
CREATE INDEX idx_products_active ON products(is_active);

-- Таблиця рівнів лояльності
CREATE TABLE loyalty_tiers (
    tier_id INTEGER PRIMARY KEY AUTOINCREMENT,
    tier_name TEXT NOT NULL,
    min_points INTEGER NOT NULL,
    discount_percentage REAL DEFAULT 0,
    points_multiplier REAL DEFAULT 1.00,
    description TEXT,
    tier_order INTEGER NOT NULL UNIQUE
);

-- Таблиця клієнтів
CREATE TABLE customers (
    customer_id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone TEXT UNIQUE NOT NULL,
    email TEXT UNIQUE,
    date_of_birth TEXT,
    registration_date TEXT DEFAULT (datetime('now','localtime')),
    is_active INTEGER DEFAULT 1,
    total_points INTEGER DEFAULT 0,
    current_tier_id INTEGER DEFAULT 1,
    FOREIGN KEY (current_tier_id) REFERENCES loyalty_tiers(tier_id)
);

CREATE INDEX idx_customers_phone ON customers(phone);
CREATE INDEX idx_customers_email ON customers(email);
CREATE INDEX idx_customers_tier ON customers(current_tier_id);

-- Таблиця замовлень
CREATE TABLE orders (
    order_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    order_date TEXT DEFAULT (datetime('now','localtime')),
    total_amount REAL NOT NULL,
    discount_amount REAL DEFAULT 0,
    final_amount REAL NOT NULL,
    points_earned INTEGER DEFAULT 0,
    points_used INTEGER DEFAULT 0,
    cashier_id INTEGER,
    receipt_number TEXT UNIQUE,
    payment_method TEXT DEFAULT 'cash' CHECK(payment_method IN ('cash', 'card', 'online')),
    order_status TEXT DEFAULT 'completed' CHECK(order_status IN ('completed', 'cancelled', 'refunded')),
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
);

CREATE INDEX idx_orders_customer ON orders(customer_id);
CREATE INDEX idx_orders_date ON orders(order_date);
CREATE INDEX idx_orders_receipt ON orders(receipt_number);
CREATE INDEX idx_orders_status ON orders(order_status);

-- Таблиця позицій замовлення
CREATE TABLE order_items (
    order_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity REAL NOT NULL,
    unit_price REAL NOT NULL,
    total_price REAL NOT NULL,
    discount_applied REAL DEFAULT 0,
    points_earned INTEGER DEFAULT 0,
    FOREIGN KEY (order_id) REFERENCES orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_product ON order_items(product_id);

-- ========================================
-- ПОЧАТКОВІ ДАНІ
-- ========================================

-- Рівні лояльності
INSERT INTO loyalty_tiers (tier_name, min_points, discount_percentage, points_multiplier, tier_order, description) VALUES
('Бронзовий', 0, 2.00, 1.00, 1, 'Початковий рівень для всіх клієнтів'),
('Срібний', 1000, 5.00, 1.25, 2, 'Досягається при накопиченні 1000 балів'),
('Золотий', 5000, 10.00, 1.50, 3, 'Преміум рівень з додатковими перевагами'),
('Платиновий', 15000, 15.00, 2.00, 4, 'VIP рівень для найактивніших клієнтів');

-- Категорії продуктів
INSERT INTO category (category_name, description, parent_category_id) VALUES
('Молочні продукти', 'Молоко, сир, йогурти', NULL),
('Хлібобулочні вироби', 'Хліб, булочки, випічка', NULL),
('Фрукти та овочі', 'Свіжі фрукти та овочі', NULL),
('М''ясо та птиця', 'Свіже та охолоджене м''ясо', NULL),
('Напої', 'Соки, вода, газовані напої', NULL),
('Бакалія', 'Крупи, макарони, консерви', NULL);

-- Приклади продуктів
INSERT INTO products (product_name, category_id, barcode, price, bonus_points_multiplier, stock_quantity, unit) VALUES
('Молоко 2.5% 1л', 1, '4820000123456', 35.50, 1.00, 150, 'шт'),
('Сир твердий 200г', 1, '4820000123457', 85.00, 1.20, 80, 'шт'),
('Йогурт натуральний 500г', 1, '4820000123458', 42.00, 1.00, 120, 'шт'),
('Хліб білий', 2, '4820000234567', 18.00, 1.00, 200, 'шт'),
('Батон нарізний', 2, '4820000234568', 22.00, 1.00, 150, 'шт'),
('Яблука Голден', 3, '4820000345678', 45.00, 1.50, 300, 'кг'),
('Банани', 3, '4820000345679', 38.00, 1.50, 250, 'кг'),
('Помідори', 3, '4820000345680', 65.00, 1.50, 180, 'кг'),
('Курка охолоджена', 4, '4820000456789', 120.00, 1.00, 100, 'кг'),
('Свинина вирізка', 4, '4820000456790', 185.00, 1.00, 60, 'кг'),
('Сік апельсиновий 1л', 5, '4820000567890', 55.00, 1.00, 200, 'шт'),
('Вода мінеральна 1.5л', 5, '4820000567891', 18.00, 1.00, 300, 'шт'),
('Гречка 1кг', 6, '4820000678901', 52.00, 1.00, 150, 'шт'),
('Макарони 400г', 6, '4820000678902', 28.00, 1.00, 200, 'шт');

-- Приклади клієнтів
INSERT INTO customers (first_name, last_name, phone, email, date_of_birth, total_points, current_tier_id) VALUES
('Іван', 'Петренко', '+380501234567', 'ivan.petrenko@email.com', '1985-03-15', 1500, 2),
('Марія', 'Коваленко', '+380502345678', 'maria.kovalenko@email.com', '1990-07-22', 450, 1),
('Олексій', 'Шевченко', '+380503456789', 'oleksiy.shevchenko@email.com', '1978-11-10', 6200, 3);

-- Приклад замовлення
INSERT INTO orders (customer_id, order_date, total_amount, discount_amount, final_amount, points_earned, points_used, receipt_number, payment_method) VALUES
(1, '2024-12-11 10:30:00', 350.00, 17.50, 332.50, 35, 0, 'RCP20241211001', 'card');

-- Позиції замовлення
INSERT INTO order_items (order_id, product_id, quantity, unit_price, total_price, discount_applied, points_earned) VALUES
(1, 1, 2, 35.50, 71.00, 3.55, 7),
(1, 6, 2.5, 45.00, 112.50, 5.63, 11),
(1, 9, 1.2, 120.00, 144.00, 7.20, 14),
(1, 11, 1, 55.00, 55.00, 1.12, 3);
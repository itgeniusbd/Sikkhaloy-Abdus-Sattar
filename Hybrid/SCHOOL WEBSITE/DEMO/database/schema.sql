CREATE DATABASE IF NOT EXISTS safa_propertis
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE safa_propertis;

CREATE TABLE IF NOT EXISTS users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(80) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS tokens (
  token VARCHAR(80) PRIMARY KEY,
  user_id INT NOT NULL,
  expires_at DATETIME NOT NULL,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS settings (
  setting_key VARCHAR(80) PRIMARY KEY,
  setting_value TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS projects (
  id VARCHAR(120) PRIMARY KEY,
  slug VARCHAR(160) NOT NULL UNIQUE,
  title VARCHAR(255) NOT NULL,
  status VARCHAR(30) NOT NULL DEFAULT 'ongoing',
  deal VARCHAR(20) NOT NULL DEFAULT 'buy',
  badge VARCHAR(40) DEFAULT '',
  category VARCHAR(80) DEFAULT '',
  type VARCHAR(40) DEFAULT 'Apartment',
  location VARCHAR(255) DEFAULT '',
  city VARCHAR(80) DEFAULT '',
  price DECIMAL(14,2) DEFAULT 0,
  beds INT DEFAULT 0,
  baths INT DEFAULT 0,
  rooms INT DEFAULT 0,
  parking INT DEFAULT 0,
  size INT DEFAULT 0,
  image TEXT,
  gallery_json TEXT,
  description TEXT,
  features_json TEXT,
  phone VARCHAR(40) DEFAULT '',
  whatsapp VARCHAR(40) DEFAULT '',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS messages (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(120) NOT NULL,
  phone VARCHAR(40) DEFAULT '',
  email VARCHAR(160) DEFAULT '',
  address VARCHAR(255) DEFAULT '',
  message TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS subscribers (
  id INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(160) NOT NULL UNIQUE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS posts (
  id VARCHAR(120) PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  category VARCHAR(80) DEFAULT '',
  image TEXT,
  excerpt TEXT,
  body TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT IGNORE INTO settings (setting_key, setting_value) VALUES
  ('phone', '+880 1712-345678'),
  ('whatsapp', '8801712345678'),
  ('email', 'info@safapropertis.com'),
  ('address', 'House #42, Road #7, Gulshan-1, Dhaka - 1212');

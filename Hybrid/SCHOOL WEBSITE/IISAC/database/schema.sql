CREATE DATABASE IF NOT EXISTS iisac_website
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE iisac_website;

CREATE TABLE IF NOT EXISTS users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(80) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  role VARCHAR(20) NOT NULL DEFAULT 'admin',
  display_name VARCHAR(120) NOT NULL DEFAULT '',
  designation VARCHAR(160) NOT NULL DEFAULT '',
  photo VARCHAR(255) NOT NULL DEFAULT '',
  permissions TEXT,
  active TINYINT NOT NULL DEFAULT 1,
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
  setting_value MEDIUMTEXT NOT NULL
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

CREATE TABLE IF NOT EXISTS admissions (
  id INT AUTO_INCREMENT PRIMARY KEY,
  status VARCHAR(20) NOT NULL DEFAULT 'pending',
  students_name VARCHAR(160) NOT NULL,
  sms_phone VARCHAR(20) NOT NULL,
  gender VARCHAR(20) DEFAULT 'Male',
  fathers_name VARCHAR(160) NOT NULL,
  education_year_id INT NOT NULL,
  education_year VARCHAR(80) DEFAULT '',
  class_id INT NOT NULL,
  class_name VARCHAR(80) DEFAULT '',
  group_id INT DEFAULT 0,
  group_name VARCHAR(80) DEFAULT '',
  section_id INT DEFAULT 0,
  section_name VARCHAR(80) DEFAULT '',
  shift_id INT DEFAULT 0,
  shift_name VARCHAR(80) DEFAULT '',
  roll_no VARCHAR(40) DEFAULT '',
  student_email VARCHAR(160) DEFAULT '',
  date_of_birth VARCHAR(20) DEFAULT '',
  legal_identity VARCHAR(80) DEFAULT '',
  blood_group VARCHAR(20) DEFAULT '',
  religion VARCHAR(40) DEFAULT '',
  permanent_address TEXT,
  local_address TEXT,
  photo VARCHAR(255) DEFAULT '',
  mothers_name VARCHAR(160) DEFAULT '',
  mother_phone VARCHAR(20) DEFAULT '',
  mother_occupation VARCHAR(120) DEFAULT '',
  father_phone VARCHAR(20) DEFAULT '',
  father_occupation VARCHAR(120) DEFAULT '',
  guardian_name VARCHAR(160) DEFAULT '',
  guardian_relation VARCHAR(80) DEFAULT '',
  guardian_phone VARCHAR(20) DEFAULT '',
  prev_school VARCHAR(200) DEFAULT '',
  prev_class VARCHAR(80) DEFAULT '',
  prev_exam_year VARCHAR(20) DEFAULT '',
  prev_grade VARCHAR(40) DEFAULT '',
  subjects_json TEXT,
  payments_json TEXT,
  assigned_id VARCHAR(40) DEFAULT '',
  sikkhaloy_student_id INT DEFAULT NULL,
  confirm_error TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  confirmed_at DATETIME NULL
);

CREATE TABLE IF NOT EXISTS jobs (
  id INT AUTO_INCREMENT PRIMARY KEY,
  title VARCHAR(200) NOT NULL,
  title_bn VARCHAR(200) DEFAULT '',
  department VARCHAR(120) DEFAULT '',
  location VARCHAR(160) DEFAULT '',
  job_type VARCHAR(40) DEFAULT 'Full-time',
  deadline VARCHAR(20) DEFAULT '',
  description TEXT,
  description_bn TEXT,
  image VARCHAR(255) DEFAULT '',
  published TINYINT NOT NULL DEFAULT 1,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS job_applications (
  id INT AUTO_INCREMENT PRIMARY KEY,
  job_id INT NOT NULL,
  name VARCHAR(160) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(160) DEFAULT '',
  address TEXT,
  education TEXT,
  experience TEXT,
  cover_letter TEXT,
  cv VARCHAR(255) DEFAULT '',
  status VARCHAR(20) NOT NULL DEFAULT 'pending',
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
  ('phone', '+880 1700-000000'),
  ('whatsapp', '8801700000000'),
  ('email', 'info@iisac.edu.bd'),
  ('address', 'Bangladesh'),
  ('school_id', '1');

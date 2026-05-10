CREATE DATABASE IF NOT EXISTS employeemangaement;
USE employeemangaement;

CREATE TABLE IF NOT EXISTS admin (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'admin'
);

CREATE TABLE IF NOT EXISTS employee (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    username VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    position VARCHAR(255) NOT NULL,
    salary DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    payroll DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    datehired VARCHAR(50) NOT NULL,
    contactno VARCHAR(50) NOT NULL,
    address TEXT NOT NULL,
    emergencycontact VARCHAR(255) NOT NULL
);

CREATE TABLE IF NOT EXISTS task (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tasktitle VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    assignedto VARCHAR(255) NOT NULL,
    duedate VARCHAR(50) NOT NULL,
    INDEX idx_task_assignedto (assignedto)
);

CREATE TABLE IF NOT EXISTS attendance (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employeename VARCHAR(255) NOT NULL,
    `date` DATE NOT NULL,
    timein VARCHAR(50) DEFAULT NULL,
    timeout VARCHAR(50) DEFAULT NULL,
    totalhours VARCHAR(50) DEFAULT NULL,
    UNIQUE KEY uq_attendance_employee_date (employeename, `date`),
    INDEX idx_attendance_date (`date`),
    INDEX idx_attendance_employee (employeename)
);

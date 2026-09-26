alter table payroll
add column payroll_month date not null;

alter table project 
add column expected_completion_date date;

insert into department (department_name) 
values ('IT') ,('HR'),('Finance'),('Sales'),('marketing'),('Operations');

INSERT INTO role (rolename)
VALUES
('Manager'),
('Developer'),
('Tester'),
('HR Executive'),
('Accountant'),
('Sales Executive'),
('Marketing Executive'),
('Team Lead');

INSERT INTO employee
(firstname, lastname, deptid, roleid, hiredate)
VALUES
('Aarav', 'Sharma', 1, 1, '2018-03-15'),
('Priya', 'Verma', 1, 2, '2019-07-10'),
('Rohan', 'Patel', 1, 2, '2021-02-20'),
('Neha', 'Singh', 1, 3, '2023-06-12'),

('Ankit', 'Gupta', 2, 1, '2017-11-05'),
('Sneha', 'Joshi', 2, 4, '2022-01-18'),

('Vikram', 'Mehta', 3, 1, '2016-09-22'),
('Kavya', 'Rao', 3, 5, '2020-04-15'),

('Rahul', 'Shah', 4, 1, '2019-01-10'),
('Pooja', 'Yadav', 4, 6, '2024-03-25'),

('Arjun', 'Malhotra', 5, 7, '2018-08-14'),
('Simran', 'Kaur', 5, 7, '2021-10-11'),

('Mohit', 'Saxena', 6, 1, '2019-05-30'),
('Isha', 'Nair', 6, 8, '2022-07-19'),

('Karan', 'Bansal', 1, 2, '2018-12-01'),
('Meera', 'Kapoor', 2, 4, '2025-01-15');


INSERT INTO project
(projectid, projectname, status, startdate, expected_completion_date)
VALUES
('P001', 'Tech Upgrade', 'Ongoing', '2026-01-10', '2026-12-31'),

('P002', 'HR Portal', 'Ongoing', '2026-02-15', '2026-10-30'),

('P003', 'Mobile App', 'Ongoing', '2026-03-01', '2026-11-15'),

('P004', 'Cloud Migration', 'Ongoing', '2026-01-20', '2026-09-30'),

('P005', 'Security Audit', 'Ongoing', '2026-04-10', '2026-10-15'),

('P006', 'Sales Dashboard', 'Completed', '2025-06-01', '2026-01-31'),

('P007', 'Payroll Automation', 'Completed', '2025-08-15', '2026-03-31'),

('P008', 'Marketing Campaign', 'Pending', '2026-08-01', '2026-12-01'),

('P009', 'Legacy Migration', 'Ongoing', '2026-02-01', '2026-06-30'),

('P010', 'Data Analytics', 'Ongoing', '2026-05-01', '2026-11-30'),

('P011', 'Customer Portal', 'Ongoing', '2026-04-01', '2026-12-15'),

('P012', 'Infrastructure Upgrade', 'Ongoing', '2026-06-01', '2026-10-31');

insert into employeeproject (emp_id,projectid,roleinproject)
values
(1, 'P001', 'Project Manager'),
(1, 'P002', 'Project Manager'),
(1, 'P003', 'Project Manager'),
(1, 'P004', 'Project Manager'),
(1, 'P005', 'Project Manager');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(3, 'P001', 'Developer'),
(3, 'P003', 'Developer'),
(3, 'P011', 'Developer');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(2, 'P001', 'Developer'),
(2, 'P003', 'Developer'),
(2, 'P004', 'Developer'),
(2, 'P010', 'Developer');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(4, 'P001', 'Tester'),
(4, 'P003', 'Tester'),
(4, 'P005', 'Tester');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(5, 'P002', 'Project Manager'),
(6, 'P002', 'HR Coordinator');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(7, 'P007', 'Project Manager'),
(8, 'P007', 'Accountant');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(9, 'P006', 'Project Manager'),
(10, 'P006', 'Sales Executive'),
(10, 'P011', 'Sales Executive');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(11, 'P008', 'Mkt Lead'),
(12, 'P008', 'Mkt Executive');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(13, 'P004', 'Project Manager'),
(14, 'P004', 'Team Lead');

INSERT INTO employeeproject
(emp_id, projectid, roleinproject)
VALUES
(5, 'P001', 'HR Manager'),
(9, 'P001', 'Business Lead'),
(11, 'P001', 'Marketing Lead'),
(13, 'P001', 'Operations Lead');


INSERT INTO attendance
(emp_id, date, checkintime, checkouttime, status)
VALUES
(1, '2026-08-03', '2026-08-03 09:00:00', '2026-08-03 18:00:00', 'Present'),
(1, '2026-08-04', '2026-08-04 08:55:00', '2026-08-04 18:00:00', 'Present'),
(1, '2026-08-05', '2026-08-05 08:50:00', '2026-08-05 18:30:00', 'Present'),
(1, '2026-08-06', '2026-08-06 09:05:00', '2026-08-06 20:30:00', 'Present'),
(1, '2026-08-07', '2026-08-07 08:45:00', '2026-08-07 18:00:00', 'Present');

INSERT INTO attendance
(emp_id, date, checkintime, checkouttime, status)
VALUES
(2, '2026-08-03', NULL, NULL, 'Absent'),
(2, '2026-08-07', NULL, NULL, 'Absent'),
(2, '2026-08-12', NULL, NULL, 'Absent'),
(2, '2026-08-18', NULL, NULL, 'Absent'),
(2, '2026-08-25', NULL, NULL, 'Absent'),

(2, '2026-08-04', '2026-08-04 09:15:00', '2026-08-04 18:00:00', 'Present'),
(2, '2026-08-05', '2026-08-05 09:20:00', '2026-08-05 18:00:00', 'Present'),
(2, '2026-08-06', '2026-08-06 09:10:00', '2026-08-06 18:00:00', 'Present'),
(2, '2026-08-10', '2026-08-10 09:30:00', '2026-08-10 18:00:00', 'Present'),
(2, '2026-08-11', '2026-08-11 09:05:00', '2026-08-11 18:00:00', 'Present'),
(2, '2026-08-13', '2026-08-13 09:25:00', '2026-08-13 18:00:00', 'Present');


INSERT INTO attendance
(emp_id, date, checkintime, checkouttime, status)
VALUES
(3, '2026-08-03', '2026-08-03 09:15:00', '2026-08-03 18:00:00', 'Present'),
(3, '2026-08-04', '2026-08-04 09:20:00', '2026-08-04 18:00:00', 'Present'),
(3, '2026-08-05', '2026-08-05 09:30:00', '2026-08-05 18:00:00', 'Present'),
(3, '2026-08-06', '2026-08-06 09:10:00', '2026-08-06 18:00:00', 'Present'),
(3, '2026-08-07', '2026-08-07 09:25:00', '2026-08-07 18:00:00', 'Present'),
(3, '2026-08-10', '2026-08-10 09:40:00', '2026-08-10 18:00:00', 'Present');

INSERT INTO attendance
(emp_id, date, checkintime, checkouttime, status)
VALUES
(4, '2026-08-04', '2026-08-04 08:30:00', '2026-08-04 19:30:00', 'Present'),
(4, '2026-08-05', '2026-08-05 08:45:00', '2026-08-05 18:00:00', 'Present'),
(4, '2026-08-06', '2026-08-06 08:30:00', '2026-08-06 20:00:00', 'Present');

INSERT INTO attendance
(emp_id, date, checkintime, checkouttime, status)
VALUES
(5, '2026-08-03', '2026-08-03 08:55:00', '2026-08-03 18:00:00', 'Present'),
(5, '2026-08-04', '2026-08-04 08:50:00', '2026-08-04 18:00:00', 'Present'),
(5, '2026-08-05', '2026-08-05 08:55:00', '2026-08-05 18:00:00', 'Present'),
(5, '2026-08-06', '2026-08-06 08:45:00', '2026-08-06 18:00:00', 'Present'),
(5, '2026-08-07', '2026-08-07 08:50:00', '2026-08-07 18:00:00', 'Present'),
(5, '2026-08-10', '2026-08-10 08:55:00', '2026-08-10 18:00:00', 'Present'),
(5, '2026-08-11', '2026-08-11 08:50:00', '2026-08-11 18:00:00', 'Present'),
(5, '2026-08-12', '2026-08-12 08:55:00', '2026-08-12 18:00:00', 'Present'),
(5, '2026-08-13', '2026-08-13 08:50:00', '2026-08-13 18:00:00', 'Present'),
(5, '2026-08-14', '2026-08-14 08:55:00', '2026-08-14 18:00:00', 'Present');

INSERT INTO payroll
(emp_id, basesalary, bonuses, deductions, netpay, payroll_month)
VALUES
(1, 100000, 10000, 5000, 105000, '2026-08-01'),

(2, 80000, 5000, 2000, 83000, '2026-08-01'),

(3, 70000, 3000, 5000, 68000, '2026-08-01'),

(4, 60000, 5000, 3000, 62000, '2026-08-01'),

(5, 90000, 10000, 5000, 95000, '2026-08-01'),

(6, 50000, 3000, 2000, 51000, '2026-08-01'),

(7, 120000, 10000, 5000, 125000, '2026-08-01'),

(8, 65000, 2000, 3000, 64000, '2026-08-01'),

(9, 85000, 5000, 4000, 86000, '2026-08-01'),

(10, 55000, 3000, 2500, 55500, '2026-08-01'),

(11, 75000, 5000, 3000, 77000, '2026-08-01'),

(12, 60000, 2000, 5000, 57000, '2026-08-01'),

(13, 95000, 5000, 4000, 96000, '2026-08-01'),

(14, 70000, 3000, 2000, 71000, '2026-08-01'),

(15, 65000, 2000, 3000, 64000, '2026-08-01'),

(16, 45000, 1000, 2000, 44000, '2026-08-01');


INSERT INTO payroll
(emp_id, basesalary, bonuses, deductions, netpay, payroll_month)
VALUES
(1, 100000, 5000, 3000, 102000, '2026-07-01'),
(2, 80000, 5000, 2000, 83000, '2026-07-01'),
(3, 70000, 3000, 2000, 71000, '2026-07-01'),
(4, 60000, 2000, 1000, 61000, '2026-07-01'),
(5, 90000, 5000, 3000, 92000, '2026-07-01');

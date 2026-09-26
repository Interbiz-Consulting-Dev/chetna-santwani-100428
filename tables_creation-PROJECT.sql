create database chetna_db;
use chetna_db;


create table department(
deptid int primary key auto_increment ,
 department_name varchar(25) not null );
 
  create table role(
 roleid int primary key auto_increment,
 rolename varchar(20) not null);
 
create table employee ( 
emp_id int primary key auto_increment,
firstname varchar(100) not null,
lastname varchar(100) not null,
deptid int ,
roleid int ,
hiredate datetime,
foreign key (deptid) references department(deptid),
foreign key(roleid) references role(roleid));

 create table project (
 projectid varchar(10) primary key ,
projectname varchar(30) not null ,
status varchar(20) not null,
startdate datetime);

create table employeeproject(
emp_id int not null,
projectid varchar(10) not null,
roleinproject varchar(15),
primary key(emp_id,projectid),
foreign key(emp_id) references employee(emp_id),
foreign key (projectid) references project(projectid));

create table attendance(
emp_id int not null,
date date not null ,
checkintime datetime ,
checkouttime datetime ,
status varchar(20),
primary key(emp_id,date),
 foreign key(emp_id) references employee(emp_id));
 
 create table payroll(
 emp_id int , 
 basesalary decimal(10,2) not null check( basesalary>0),
 bonuses decimal(10,2) default 0 ,
 deductions decimal(10,2) default 0,
 Netpay decimal(10,2) ,
  foreign key(emp_id) references employee(emp_id));
 
 
  
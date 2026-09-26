-- 3. Department Salary Analysis 

-- "Provide a report that shows the total and average salary (NetPay) for employees in each department. Include department names in your results." 

select * from payroll;

select d.department_name ,sum(p.netpay),avg(p.netpay)
from department d 
inner join  employee e
on d.deptid=e.deptid
inner join payroll p 
on e.emp_id=p.emp_id
where p.payroll_month='2026-08-01'
group by d.deptid , d.department_name;

-- 4. Employee Without Projects 

-- "Find employees who are not currently assigned to any project. Display their names, department names, and roles." 

select * from employeeproject;

select e.emp_id ,r.rolename,e.firstname,d.department_name
from employee e 
inner join department d 
on e.deptid=d.deptid
inner join role r on
e.roleid=r.roleid
where not exists
(select ep.emp_id 
from employeeproject ep
where ep.emp_id=e.emp_id);


-- 5. Project Assignment Role 
-- "Retrieve a list of employees assigned to a project named 'Tech Upgrade' and their roles in the project. Ensure the project is still ongoing." 

select * from project;
select * from employeeproject;

select e.emp_id,ep.roleinproject
from employee e 
inner join 
employeeproject ep 
on e.emp_id =ep.emp_id
where ep.projectid in (
select p.projectid from project p
where p.projectname='Tech Upgrade' and p.status='Ongoing');

select e.firstname,ep.roleinproject ,e.emp_id
from employee e 
inner join 
employeeproject ep 
on e.emp_id=ep.emp_id
where exists  (
select 1 from project p 
where p.projectid=ep.projectid
and p.projectname='Tech Upgrade'
and p.status='Ongoing');


-- 6. Payroll Verification 
-- "Identify employees whose net pay (NetPay) is less than 60% of their base salary (BaseSalary). Display their names and corresponding percentages. Result should for selected month" 
select e.emp_id,e.firstname,p.basesalary,p.Netpay,(p.netpay/p.basesalary)*100 as netpay_percentage
from employee e 
inner join payroll p 
on e.emp_id=p.emp_id 
where p.Netpay< p.basesalary*0.6
and p.payroll_month='26-08-01';

-- 7. Department Hire Dates 
-- "Find the earliest hire date of employees in each department and the names of employees hired on those dates." 

select e.firstname,d.department_name,e.hiredate
from employee e
inner join   (
select min(hiredate) as earliest_hiredate,deptid
from employee 
group by deptid) x 
on e.deptid=x.deptid
and e.hiredate=x.earliest_hiredate
inner join department d
on e.deptid=d.deptid;

-- 8. Long Working Hours 
-- "Create a report of employees who worked more than 10 hours on any given day last month. Include their names, the date, and total hours worked." 

select * from attendance;


SELECT e.firstname,e.lastname,
		a.date,
    CONCAT(
        TIMESTAMPDIFF(MINUTE, a.checkintime, a.checkouttime) DIV 60,
        ' hrs ',
        TIMESTAMPDIFF(MINUTE, a.checkintime, a.checkouttime) MOD 60,
        ' mins'
    ) AS working_hours
FROM attendance a
INNER JOIN employee e
    ON a.emp_id = e.emp_id
WHERE a.date >= '2026-08-01'
  AND a.date < '2026-09-01'
  AND TIMESTAMPDIFF(MINUTE, a.checkintime, a.checkouttime) > 600;

-- 9. Department Leaders  
-- "Display the names of department managers (employees whose RoleID corresponds to 'Manager')  along with their department names.
select * from role;

select e.firstname ,d.department_name
from employee e 
inner join 
department d
on e.deptid=d.deptid 
 where exists(
 select 1 from 
 role r where r.roleid=e.roleid
 and r.rolename='Manager');

-- 10. Salary Increment Plan 
-- "Management wants to give a 10% salary increment to all employees hired before 2020. Write a query to update the BaseSalary in the Payroll table and list the updated salaries."  


SET SQL_SAFE_UPDATES = 0;
update payroll 
SET basesalary =  1.10 * basesalary
where emp_id in (
select emp_id 
from employee
where hiredate < '2020-01-01')
and payroll_month='26-08-01';
SET SQL_SAFE_UPDATES = 1; 


SELECT e.emp_id,
       e.firstname,
       e.lastname,
       e.hiredate,
       p.basesalary AS updated_salary
FROM employee e
JOIN payroll p ON e.emp_id = p.emp_id
WHERE e.hiredate < '2020-01-01'
  AND p.payroll_month = '2026-08-01';

-- Employee Experience Report: Find the total experience (in years) of all employees grouped by department. Include department names and average employee experience. -- 
use chetna_db;
SELECT d.department_name ,sum(timestampdiff(year,e.hiredate,curdate())) as total_experience ,avg(timestampdiff(year,e.hiredate,curdate()))as avg_experience
FROM employee e 
inner join 
department d
on e.deptid=d.deptid
group by d.department_name;

-- Project Status Overview: Write a query to count the number of projects in each status (e.g., Ongoing, Completed, Pending). 

select count(*) ,status
from project
group by status;

-- Late Check-In Report: Identify employees who checked in late (after 9:00 AM) more than 5 times in the past month. Display their names and the dates they were late. 

select e.firstname,a.date 
from employee e 
join 
attendance a 
on e.emp_id=a.emp_id
WHERE TIME(a.checkintime) > '09:00:00'
  AND a.date >= '2026-08-01'
  AND a.date < '2026-09-01'
and exists(
select 1 
from attendance a1
where a1.emp_id=e.emp_id
and time(a1.checkintime)>'9:00'
 AND a1.date >= '2026-08-01'
and a1.date <'26-09-01' 
group by a1.emp_id
having count(*) >5);

-- Inactive Employees: Find employees who have not checked in or checked out in the last 3 months. List their names and departments.
select e.firstname,d.department_name
from employee e 
join 
department d 
on e.deptid=d.deptid
where not exists(
select 1 
from attendance a1
where a1.emp_id=e.emp_id
and a1.checkintime is not null or a1.checkouttime is not null 
and a1.date>=date_sub(curdate(),interval 3 month)
);

-- Employee Role Statistics: Write a query to calculate the number of employees in each role and display it alongside the role names. 
select r.roleid,r.rolename,count(e.emp_id)
from role r
left join 
employee e
on r.roleid=e.roleid
group by r.roleid , r.rolename;

-- Overdue Projects: List all projects that are overdue based on their expected completion date. Include project names, current status, and the number of overdue days
select * from project;

select projectname ,status ,datediff(curdate(),expected_completion_date)
from project 
where expected_completion_date<curdate()
and status <> 'Completed';


-- Highest Paid Employees: Retrieve the top 5 highest-paid employees in the organization along with their departments and roles. 
select e.firstname ,d.department_name,r.rolename
from 
employee e 
join 
department d
on e.deptid=d.deptid
join 
role r 
on r.roleid=e.roleid
join payroll p 
on p.emp_id=e.emp_id
order by p.netpay desc 
limit 5;


 


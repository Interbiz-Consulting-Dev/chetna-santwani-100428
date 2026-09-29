use chetna_db;
-- Cross-Department Projects: Identify projects that involve employees from more than 3 different departments. List the project names and involved department counts. 
select p.projectname,count(*)
 from employee e 
 join department d
 on e.deptid=d.deptid
join employeeproject ep on
ep.emp_id=e.emp_id
join project p 
on p.projectid=ep.projectid
group by ep.projectid,d.department_name
having count(*)>3;

-- Employee Attendance Patterns: Identify employees with consistent attendance (e.g., marked 'Present' for more than 90% of working days in the past month). Include their names and attendance percentages
select * from attendance;
select e.firstname,e.lastname,ROUND(sum(a.status='Present')/count(*) *100,2) as attendance_percentage
from employee e 
join attendance a 
on e.emp_id=a.emp_id
where a.date>='2026-08-01' and a.date<'2026-09-01'
group by e.emp_id,e.firstname,e.lastname
having attendance_percentage>90;








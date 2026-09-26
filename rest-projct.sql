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









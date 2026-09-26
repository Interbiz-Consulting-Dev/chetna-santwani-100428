-- 1. Identify High-Performing Employees 
-- "Management wants to know which employees have worked on more than three active projects (Status = 'Ongoing'). Write a query to list the names of these employees with project name." 


 USE chetna_db;

select e.emp_id,p.projectname,p.status
from employee e
inner join 
employeeproject ep
on e.emp_id=ep.emp_id
inner join project p 
on ep.projectid=p.projectid
where p.status='Ongoing' and e.emp_id in (

select ep1.emp_id from 
employeeproject ep1
inner join project p1
on ep1.projectid=p1.projectid
where p1.status='Ongoing'
 group by ep1.emp_id 
 having  count(*)>3


);
-- 2. Monitor Employee Attendance 

-- "Generate a report of employees who were marked 'Absent' more than three times in the last month. Include their names and the total number of absences with absence dates."

select * from attendance;

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
(7, '2026-09-03', NULL, NULL, 'Absent'),
(7, '2026-09-07', NULL, NULL, 'Absent'),
(7, '2026-08-04', '2026-08-04 09:15:00', '2026-08-04 18:00:00', 'Present'),
(7, '2026-08-05', '2026-08-05 09:20:00', '2026-08-05 18:00:00', 'Present');

select e.emp_id,e.firstname,a.date
from attendance a 
inner join 
employee e 
on a.emp_id=e.emp_id 
 where a.status='Absent' and e.emp_id in 
 (
 select a1.emp_id
from attendance a1 
where status='Absent'
group by month(a1.date),year(a1.date) ,a1.emp_id
having count(*)>3
 )
 ;
 
 



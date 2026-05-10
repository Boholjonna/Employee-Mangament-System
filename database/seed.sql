USE employeemangaement;

INSERT INTO admin (name, username, email, password, role)
VALUES
    ('System Administrator', 'root', 'root@local', 'computerengineering', 'admin')
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    email = VALUES(email),
    password = VALUES(password),
    role = VALUES(role);

INSERT INTO employee (name, username, password, position, salary, payroll, datehired, contactno, address, emergencycontact)
VALUES
    ('Alice Santos', 'alice.santos', 'password123', 'Software Engineer', 65000.00, 5400.00, '2024-01-15', '09171234567', 'Makati City', 'Maria Santos'),
    ('Bob Reyes', 'bob.reyes', 'password123', 'Project Manager', 78000.00, 6500.00, '2023-09-01', '09179876543', 'Quezon City', 'Jose Reyes'),
    ('Carol Lim', 'carol.lim', 'password123', 'QA Analyst', 52000.00, 4300.00, '2024-03-10', '09175551234', 'Pasig City', 'Anna Lim'),
    ('Jonna Cruz', 'jonna@gmail.com', '123', 'HR Assistant', 45000.00, 3750.00, '2025-05-10', '09170000001', 'Taguig City', 'Lorna Cruz')
ON DUPLICATE KEY UPDATE
    password = VALUES(password),
    position = VALUES(position),
    salary = VALUES(salary),
    payroll = VALUES(payroll),
    datehired = VALUES(datehired),
    contactno = VALUES(contactno),
    address = VALUES(address),
    emergencycontact = VALUES(emergencycontact);

INSERT INTO task (id, tasktitle, description, assignedto, duedate)
VALUES
    (1, 'Complete onboarding docs', 'Finish the employee onboarding checklist.', 'Alice Santos', '2026-05-20'),
    (2, 'Review sprint backlog', 'Validate tasks for the upcoming sprint.', 'Bob Reyes', '2026-05-22'),
    (3, 'Run regression tests', 'Execute regression suite and log defects.', 'Carol Lim', '2026-05-24')
ON DUPLICATE KEY UPDATE
    description = VALUES(description),
    assignedto = VALUES(assignedto),
    duedate = VALUES(duedate);

DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = 'admin@gmail.com');
DELETE FROM AspNetUsers WHERE Email = 'admin@gmail.com';
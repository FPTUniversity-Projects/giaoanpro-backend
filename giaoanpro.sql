/*
================================================================================
 EXTENDED SEED SCRIPT FOR GIAOANPRO DB (DỮ LIỆU MẪU MỞ RỘNG)
 ================================================================================
 - Môn học: Ngữ Văn (6-12)
 - Users: 1 Admin, 5 Teachers, 10 Students
 - Classes: 5 Lớp học khác nhau
 - Lesson Plans: Nhiều bài giáo án mẫu
================================================================================
*/
USE [GiaoAnProDB]
GO

-- 1. DỌN DẸP DỮ LIỆU CŨ
DELETE FROM [dbo].[AttemptDetails];
DELETE FROM [dbo].[Attempts];
DELETE FROM [dbo].[ExamQuestions];
DELETE FROM [dbo].[Exams];
DELETE FROM [dbo].[ExamMatrices];
DELETE FROM [dbo].[QuestionOptions];
DELETE FROM [dbo].[QuestionAttributes];
DELETE FROM [dbo].[Questions];
DELETE FROM [dbo].[Activitys];
DELETE FROM [dbo].[LessonPlans];
DELETE FROM [dbo].[ClassMembers];
DELETE FROM [dbo].[Classes];
ALTER TABLE [dbo].[Subjects] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Syllabuses] NOCHECK CONSTRAINT ALL;
DELETE FROM [dbo].[Syllabuses];
DELETE FROM [dbo].[Subjects];
ALTER TABLE [dbo].[Subjects] CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[Syllabuses] CHECK CONSTRAINT ALL;
DELETE FROM [dbo].[Grades];
DELETE FROM [dbo].[Semesters];
DELETE FROM [dbo].[Payments];
DELETE FROM [dbo].[Subscriptions];
DELETE FROM [dbo].[SubscriptionPlans];
DELETE FROM [dbo].[PromptLogs];
DELETE FROM [dbo].[Users];
DELETE FROM [dbo].[Attributes];
GO

-- 2. KHAI BÁO BIẾN CHUNG
DECLARE @Now DATETIME2 = GETDATE();
DECLARE @PasswordHash NVARCHAR(MAX) = N'OjPKoiQjDRrVdhfCMtuxCkisBI8Lrh7nmBoOz624/Vg='; 

-- 3. ATTRIBUTES
INSERT INTO [dbo].[Attributes] ([Id], [Type], [Value], [CreatedAt], [UpdatedAt]) VALUES
(NEWID(), 1, N'Theory', @Now, @Now), (NEWID(), 1, N'Exercise', @Now, @Now),
(NEWID(), 2, N'Easy', @Now, @Now), (NEWID(), 2, N'Medium', @Now, @Now), (NEWID(), 2, N'Hard', @Now, @Now),
(NEWID(), 3, N'Remember', @Now, @Now), (NEWID(), 3, N'Understand', @Now, @Now), (NEWID(), 3, N'Apply', @Now, @Now), (NEWID(), 3, N'AdvancedApply', @Now, @Now);

-- 4. GRADES & SEMESTERS
DECLARE @G6 UNIQUEIDENTIFIER = NEWID(); DECLARE @G7 UNIQUEIDENTIFIER = NEWID();
DECLARE @G8 UNIQUEIDENTIFIER = NEWID(); DECLARE @G9 UNIQUEIDENTIFIER = NEWID();
DECLARE @G10 UNIQUEIDENTIFIER = NEWID(); DECLARE @G11 UNIQUEIDENTIFIER = NEWID();
DECLARE @G12 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Grades] ([Id], [Level], [CreatedAt], [UpdatedAt]) VALUES
(@G6, 6, @Now, @Now), (@G7, 7, @Now, @Now), (@G8, 8, @Now, @Now), (@G9, 9, @Now, @Now),
(@G10, 10, @Now, @Now), (@G11, 11, @Now, @Now), (@G12, 12, @Now, @Now);

DECLARE @Sem1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Sem2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Semesters] ([Id], [Name], [StartDate], [EndDate], [CreatedAt], [UpdatedAt]) VALUES
(@Sem1, N'Học kỳ I (2024-2025)', DATEADD(month, -2, @Now), DATEADD(month, 3, @Now), @Now, @Now),
(@Sem2, N'Học kỳ II (2024-2025)', DATEADD(month, 3, @Now), DATEADD(month, 8, @Now), @Now, @Now);

-- 5. USERS (1 Admin, 5 Teachers, 10 Students)
DECLARE @Admin UNIQUEIDENTIFIER = NEWID();
DECLARE @T_Active UNIQUEIDENTIFIER = NEWID(); -- Cô Hạnh (Active)
DECLARE @T_Canceled UNIQUEIDENTIFIER = NEWID(); -- Thầy Hùng (Canceled - Vẫn dùng đc)
DECLARE @T_Expired UNIQUEIDENTIFIER = NEWID(); -- Cô Lan (Expired - Hết hạn)
DECLARE @T_New UNIQUEIDENTIFIER = NEWID(); -- Thầy Mới (Chưa mua gói nào)
DECLARE @T_Pro UNIQUEIDENTIFIER = NEWID(); -- Thầy VIP (Gói năm)

INSERT INTO [dbo].[Users] ([Id], [Username], [PasswordHash], [Email], [FullName], [IsActive], [Role], [CreatedAt], [UpdatedAt], [RefreshToken], [RefreshTokenExpiryTime]) VALUES
(@Admin, N'admin', @PasswordHash, N'admin@giaoan.pro', N'Quản Trị Viên', 1, N'Admin', @Now, @Now, N'', NULL),
(@T_Active, N'hanh_active', @PasswordHash, N'hanh@giaoan.pro', N'Cô Hạnh (Văn 10)', 1, N'Teacher', @Now, @Now, N'', NULL),
(@T_Canceled, N'hung_cancel', @PasswordHash, N'hung@giaoan.pro', N'Thầy Hùng (Văn 12)', 1, N'Teacher', @Now, @Now, N'', NULL),
(@T_Expired, N'lan_expired', @PasswordHash, N'lan@giaoan.pro', N'Cô Lan (Văn 6)', 1, N'Teacher', @Now, @Now, N'', NULL),
(@T_New, N'minh_new', @PasswordHash, N'minh@giaoan.pro', N'Thầy Minh (Mới)', 1, N'Teacher', @Now, @Now, N'', NULL),
(@T_Pro, N'dung_pro', @PasswordHash, N'dung@giaoan.pro', N'Thầy Dũng (VIP)', 1, N'Teacher', @Now, @Now, N'', NULL);

-- Insert 10 Students
DECLARE @S1 UNIQUEIDENTIFIER = NEWID(); DECLARE @S2 UNIQUEIDENTIFIER = NEWID();
DECLARE @S3 UNIQUEIDENTIFIER = NEWID(); DECLARE @S4 UNIQUEIDENTIFIER = NEWID();
DECLARE @S5 UNIQUEIDENTIFIER = NEWID(); DECLARE @S6 UNIQUEIDENTIFIER = NEWID();
DECLARE @S7 UNIQUEIDENTIFIER = NEWID(); DECLARE @S8 UNIQUEIDENTIFIER = NEWID();
DECLARE @S9 UNIQUEIDENTIFIER = NEWID(); DECLARE @S10 UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[Users] ([Id], [Username], [PasswordHash], [Email], [FullName], [IsActive], [Role], [CreatedAt], [UpdatedAt], [RefreshToken], [RefreshTokenExpiryTime]) VALUES
(@S1, N'student1', @PasswordHash, N's1@school.com', N'Nguyễn Văn An', 1, N'Student', @Now, @Now, N'', NULL),
(@S2, N'student2', @PasswordHash, N's2@school.com', N'Trần Thị Bích', 1, N'Student', @Now, @Now, N'', NULL),
(@S3, N'student3', @PasswordHash, N's3@school.com', N'Lê Văn Cường', 1, N'Student', @Now, @Now, N'', NULL),
(@S4, N'student4', @PasswordHash, N's4@school.com', N'Phạm Thị Dung', 1, N'Student', @Now, @Now, N'', NULL),
(@S5, N'student5', @PasswordHash, N's5@school.com', N'Hoàng Văn Em', 1, N'Student', @Now, @Now, N'', NULL),
(@S6, N'student6', @PasswordHash, N's6@school.com', N'Đỗ Thị Fương', 1, N'Student', @Now, @Now, N'', NULL),
(@S7, N'student7', @PasswordHash, N's7@school.com', N'Vũ Văn Giàu', 1, N'Student', @Now, @Now, N'', NULL),
(@S8, N'student8', @PasswordHash, N's8@school.com', N'Ngô Thị Hoa', 1, N'Student', @Now, @Now, N'', NULL),
(@S9, N'student9', @PasswordHash, N's9@school.com', N'Bùi Văn Ích', 1, N'Student', @Now, @Now, N'', NULL),
(@S10, N'student10', @PasswordHash, N's10@school.com', N'Lý Thị Ka', 1, N'Student', @Now, @Now, N'', NULL);

-- 6. PLANS & SUBSCRIPTIONS
DECLARE @Plan_M UNIQUEIDENTIFIER = NEWID(); -- Monthly
DECLARE @Plan_Y UNIQUEIDENTIFIER = NEWID(); -- Yearly

INSERT INTO [dbo].[SubscriptionPlans] VALUES 
(@Plan_M, N'Gói Tháng', N'Cơ bản', 50000, 30, @Now, @Now, NULL, 1, 30, 50),
(@Plan_Y, N'Gói Năm', N'Tiết kiệm', 500000, 365, @Now, @Now, NULL, 1, 1000, 500);

-- Cô Hạnh (Active - Month)
DECLARE @Sub1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Subscriptions] VALUES (@Sub1, @T_Active, @Plan_M, DATEADD(d,-5,@Now), DATEADD(d,25,@Now), N'Active', @Now, @Now, NULL, 5, 20, @Now);
INSERT INTO [dbo].[Payments] VALUES (NEWID(), @Sub1, 50000, DATEADD(d,-5,@Now), N'Success', N'VNPAY', N'TX01', NULL, @Now, @Now, N'Mua gói tháng', N'00');

-- Thầy Hùng (Canceled - Month - Còn hạn)
DECLARE @Sub2 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Subscriptions] VALUES (@Sub2, @T_Canceled, @Plan_M, DATEADD(d,-10,@Now), DATEADD(d,20,@Now), N'Canceled', @Now, @Now, NULL, 28, 40, @Now); -- Sắp hết limit lesson
INSERT INTO [dbo].[Payments] VALUES (NEWID(), @Sub2, 50000, DATEADD(d,-10,@Now), N'Success', N'VNPAY', N'TX02', NULL, @Now, @Now, N'Mua gói tháng', N'00');

-- Cô Lan (Expired - Month - Hết hạn)
DECLARE @Sub3 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Subscriptions] VALUES (@Sub3, @T_Expired, @Plan_M, DATEADD(d,-40,@Now), DATEADD(d,-10,@Now), N'Expired', @Now, @Now, NULL, 30, 50, DATEADD(d,-11,@Now));
INSERT INTO [dbo].[Payments] VALUES (NEWID(), @Sub3, 50000, DATEADD(d,-40,@Now), N'Success', N'VNPAY', N'TX03', NULL, @Now, @Now, N'Mua gói tháng', N'00');

-- Thầy Dũng (Active - Year)
DECLARE @Sub4 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Subscriptions] VALUES (@Sub4, @T_Pro, @Plan_Y, DATEADD(d,-100,@Now), DATEADD(d,265,@Now), N'Active', @Now, @Now, NULL, 150, 1200, @Now);
INSERT INTO [dbo].[Payments] VALUES (NEWID(), @Sub4, 500000, DATEADD(d,-100,@Now), N'Success', N'VNPAY', N'TX04', NULL, @Now, @Now, N'Mua gói năm', N'00');

-- 7. SUBJECTS & SYLLABUSES
DECLARE @Subj_10 UNIQUEIDENTIFIER = NEWID(); DECLARE @Subj_12 UNIQUEIDENTIFIER = NEWID(); DECLARE @Subj_6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Syl_CD UNIQUEIDENTIFIER = NEWID(); -- Cánh Diều
DECLARE @Syl_KNTT UNIQUEIDENTIFIER = NEWID(); -- Kết Nối Tri Thức
DECLARE @Syl_CTST UNIQUEIDENTIFIER = NEWID(); -- Chân Trời Sáng Tạo

-- Tạo Subject trước
INSERT INTO [dbo].[Subjects] VALUES (@Subj_10, @G10, @Syl_CD, N'Ngữ Văn 10', N'Sách Cánh Diều', @Now, @Now);
INSERT INTO [dbo].[Subjects] VALUES (@Subj_12, @G12, @Syl_KNTT, N'Ngữ Văn 12', N'Sách KNTT', @Now, @Now);
INSERT INTO [dbo].[Subjects] VALUES (@Subj_6, @G6, @Syl_CTST, N'Ngữ Văn 6', N'Sách CTST', @Now, @Now);

-- Tạo Syllabus sau
INSERT INTO [dbo].[Syllabuses] VALUES (@Syl_CD, @Subj_10, N'Cánh Diều', N'Sách Cánh Diều 2025', @Now, @Now, NULL);
INSERT INTO [dbo].[Syllabuses] VALUES (@Syl_KNTT, @Subj_12, N'Kết Nối Tri Thức', N'Sách KNTT 2025', @Now, @Now, NULL);
INSERT INTO [dbo].[Syllabuses] VALUES (@Syl_CTST, @Subj_6, N'Chân Trời Sáng Tạo', N'Sách CTST 2025', @Now, @Now, NULL);

-- 8. CLASSES & MEMBERS
DECLARE @Class_10A UNIQUEIDENTIFIER = NEWID(); -- Cô Hạnh
DECLARE @Class_12B UNIQUEIDENTIFIER = NEWID(); -- Thầy Hùng
DECLARE @Class_6C UNIQUEIDENTIFIER = NEWID(); -- Cô Lan (Hết hạn sub - vẫn còn lớp)

INSERT INTO [dbo].[Classes] VALUES (@Class_10A, @T_Active, @G10, @Sem1, N'Lớp 10A - Chuyên Văn', @Now, @Now);
INSERT INTO [dbo].[Classes] VALUES (@Class_12B, @T_Canceled, @G12, @Sem1, N'Lớp 12B - Ôn Thi ĐH', @Now, @Now);
INSERT INTO [dbo].[Classes] VALUES (@Class_6C, @T_Expired, @G6, @Sem1, N'Lớp 6C - Đại Trà', @Now, @Now);

-- Đăng ký học sinh (Enroll)
-- Lớp 10A (3 HS)
INSERT INTO [dbo].[ClassMembers] VALUES (@S1, @Class_10A, @Now, @Now), (@S2, @Class_10A, @Now, @Now), (@S3, @Class_10A, @Now, @Now);
-- Lớp 12B (3 HS)
INSERT INTO [dbo].[ClassMembers] VALUES (@S4, @Class_12B, @Now, @Now), (@S5, @Class_12B, @Now, @Now), (@S6, @Class_12B, @Now, @Now);
-- Lớp 6C (2 HS)
INSERT INTO [dbo].[ClassMembers] VALUES (@S7, @Class_6C, @Now, @Now), (@S8, @Class_6C, @Now, @Now);

-- 9. LESSON PLANS (Giáo án)
DECLARE @LP1 UNIQUEIDENTIFIER = NEWID(); -- Cô Hạnh (Văn 10)
DECLARE @LP2 UNIQUEIDENTIFIER = NEWID(); -- Cô Hạnh (Văn 10 - Bài khác)
DECLARE @LP3 UNIQUEIDENTIFIER = NEWID(); -- Thầy Hùng (Văn 12)

INSERT INTO [dbo].[LessonPlans] VALUES 
(@LP1, @T_Active, @Subj_10, N'Chiến thắng Mtao Mxây', N'Hiểu về sử thi Tây Nguyên', N'Dùng máy chiếu', @Now, @Now),
(@LP2, @T_Active, @Subj_10, N'Truyện An Dương Vương', N'Bài học giữ nước', N'Thảo luận nhóm', @Now, @Now),
(@LP3, @T_Canceled, @Subj_12, N'Tuyên Ngôn Độc Lập', N'Giá trị lịch sử', N'Phân tích văn bản', @Now, @Now);

-- 10. ACTIVITIES & QUESTIONS
DECLARE @Act1 UNIQUEIDENTIFIER = NEWID(); -- Startup cho LP1
DECLARE @Q1 UNIQUEIDENTIFIER = NEWID(); 
DECLARE @Q2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Q3 UNIQUEIDENTIFIER = NEWID(); -- Câu hỏi cho LP3

INSERT INTO [dbo].[Activitys] VALUES (@Act1, @LP1, N'Startup', N'Khởi động', N'Xem video', N'Cảm nghĩ HS', N'Chiếu video', @Now, @Now, NULL, N'HĐ1: Xem Video');

INSERT INTO [dbo].[Questions] VALUES 
(@Q1, @LP1, N'Đăm Săn là tù trưởng bộ lạc nào?', @Now, @Now, N'Remember', NULL, N'Easy', NULL, N'Theory'),
(@Q2, @LP1, N'Vì sao Đăm Săn chiến đấu với Mtao Mxây?', @Now, @Now, N'Understand', NULL, N'Medium', NULL, N'Theory'),
(@Q3, @LP3, N'Bác Hồ đọc TNĐL ngày nào?', @Now, @Now, N'Remember', NULL, N'Easy', NULL, N'Theory');

-- Options
INSERT INTO [dbo].[QuestionOptions] VALUES
(NEWID(), @Q1, N'Ê-đê', 1, @Now, @Now, NULL), (NEWID(), @Q1, N'Mơ-nông', 0, @Now, @Now, NULL),
(NEWID(), @Q2, N'Đòi lại vợ', 1, @Now, @Now, NULL), (NEWID(), @Q2, N'Tranh giành đất đai', 0, @Now, @Now, NULL),
(NEWID(), @Q3, N'2/9/1945', 1, @Now, @Now, NULL), (NEWID(), @Q3, N'19/8/1945', 0, @Now, @Now, NULL);

-- 11. EXAMS & ATTEMPTS
DECLARE @Matrix UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[ExamMatrices] VALUES (@Matrix, @Subj_10, @LP1, N'Kiểm tra 15p', 2, 5, @Now, @Now);

DECLARE @Exam UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Exams] VALUES (@Exam, @Matrix, @Act1, @T_Active, N'KT 15 Phút - Sử Thi', N'Làm bài nhanh', 15, @Now, @Now);

INSERT INTO [dbo].[ExamQuestions] VALUES (@Exam, @Q1, 1, @Now, @Now, NULL), (@Exam, @Q2, 2, @Now, @Now, NULL);

-- HS1 làm bài
DECLARE @Att1 UNIQUEIDENTIFIER = NEWID();
INSERT INTO [dbo].[Attempts] VALUES (@Att1, @S1, @Exam, DATEADD(m,-20,@Now), DATEADD(m,-5,@Now), 10, N'Graded', @Now, @Now);
INSERT INTO [dbo].[AttemptDetails] VALUES (NEWID(), @Att1, @Q1, N'Ê-đê', 1, 5, N'Đúng', @Now, @Now, NULL), (NEWID(), @Att1, @Q2, N'Đòi lại vợ', 1, 5, N'Đúng', @Now, @Now, NULL);

PRINT 'Extended Seed Data Inserted Successfully!'
GO
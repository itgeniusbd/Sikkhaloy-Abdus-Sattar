-- =====================================================
-- Word of the Day Database - Simplified Version
-- =====================================================
-- Database: Edu
-- Table: WordOfTheDay
-- Total Words: 200+ (Sample Set)
-- Format: No Category column
-- =====================================================

USE [Edu]
GO

-- =====================================================
-- Step 1: Create Table (if not exists)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WordOfTheDay]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WordOfTheDay](
        [WordID] [int] IDENTITY(1,1) NOT NULL,
   [EnglishWord] [nvarchar](100) NOT NULL,
        [BengaliMeaning] [nvarchar](200) NOT NULL,
 [PartOfSpeech] [nvarchar](50) NOT NULL,
        [ExampleSentence] [nvarchar](500) NOT NULL,
        [Pronunciation] [nvarchar](100) NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT (GETDATE()),
      [IsActive] [bit] NOT NULL DEFAULT (1),
  CONSTRAINT [PK_WordOfTheDay] PRIMARY KEY CLUSTERED ([WordID] ASC)
    )
    
    PRINT 'Table WordOfTheDay created successfully!'
END
ELSE
BEGIN
    PRINT 'Table WordOfTheDay already exists!'
    
    -- If Category column exists, drop it
    IF EXISTS (
        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'WordOfTheDay' 
        AND COLUMN_NAME = 'Category'
    )
    BEGIN
        ALTER TABLE [dbo].[WordOfTheDay] DROP COLUMN [Category];
  PRINT 'Category column removed from existing table!'
    END
END
GO

-- =====================================================
-- Step 2: Insert Sample Data (200 words)
-- =====================================================
PRINT 'Inserting sample words...'
GO

INSERT INTO [dbo].[WordOfTheDay]
(EnglishWord, BengaliMeaning, PartOfSpeech, ExampleSentence, Pronunciation)
VALUES
-- A
(N'Ability', N'ক্ষমতা, সামর্থ্য', N'Noun', N'She has the ability to lead.', N'uh-BIL-i-tee'),
(N'Academic', N'শিক্ষাগত, একাডেমিক', N'Adjective', N'Focus on your academic performance.', N'ak-uh-DEM-ik'),
(N'Achievement', N'অর্জন, সাফল্য', N'Noun', N'Celebrate your achievements.', N'uh-CHEEV-muhnt'),
(N'Active', N'সক্রিয়, কর্মঠ', N'Adjective', N'Stay active and healthy.', N'AK-tiv'),
(N'Actually', N'প্রকৃতপক্ষে, আসলে', N'Adverb', N'Actually, I agree with you.', N'AK-choo-uh-lee'),
(N'Admit', N'স্বীকার করা, ভর্তি হওয়া', N'Verb', N'He had to admit his mistake.', N'ad-MIT'),
(N'Advance', N'অগ্রসর হওয়া, উন্নতি', N'Verb', N'Technology will advance rapidly.', N'ad-VANS'),
(N'Advice', N'পরামর্শ, উপদেশ', N'Noun', N'Take advice from your elders.', N'ad-VAHYS'),
(N'Agree', N'একমত হওয়া, সম্মত হওয়া', N'Verb', N'I agree with your opinion.', N'uh-GREE'),
(N'Allow', N'অনুমতি দেওয়া, মঞ্জুর করা', N'Verb', N'Parents allow children to play.', N'uh-LOU'),

-- B
(N'Balance', N'ভারসাম্য, সমতা', N'Noun', N'Maintain work-life balance.', N'BAL-uhns'),
(N'Basic', N'মৌলিক, প্রাথমিক', N'Adjective', N'Learn the basic rules first.', N'BEY-sik'),
(N'Beautiful', N'সুন্দর, মনোরম', N'Adjective', N'She has a beautiful smile.', N'BYOO-tuh-fuhl'),
(N'Believe', N'বিশ্বাস করা, মানা', N'Verb', N'Believe in yourself always.', N'bih-LEEV'),
(N'Benefit', N'লাভ, উপকার', N'Noun', N'Exercise has many benefits.', N'BEN-uh-fit'),
(N'Better', N'উত্তম, ভালো', N'Adjective', N'Practice makes you better.', N'BET-er'),
(N'Brave', N'সাহসী, বীর', N'Adjective', N'Be brave in difficult times.', N'BREYV'),
(N'Bright', N'উজ্জ্বল, প্রতিভাবান', N'Adjective', N'She is a bright student.', N'BRAHYT'),
(N'Build', N'নির্মাণ করা, গড়া', N'Verb', N'Build your future carefully.', N'BILD'),
(N'Busy', N'ব্যস্ত, কর্মব্যস্ত', N'Adjective', N'He is busy with his work.', N'BIZ-ee'),

-- C
(N'Capable', N'সক্ষম, যোগ্য', N'Adjective', N'You are capable of success.', N'KEY-puh-buhl'),
(N'Care', N'যত্ন, পরিচর্যা', N'Noun', N'Take care of your health.', N'KAIR'),
(N'Careful', N'সতর্ক, যত্নশীল', N'Adjective', N'Be careful while crossing roads.', N'KAIR-fuhl'),
(N'Celebrate', N'উদযাপন করা, পালন করা', N'Verb', N'Celebrate your success.', N'SEL-uh-breyt'),
(N'Challenge', N'চ্যালেঞ্জ, সমস্যা', N'Noun', N'Face every challenge bravely.', N'CHAL-inj'),
(N'Change', N'পরিবর্তন, বদল', N'Noun', N'Change is the only constant.', N'CHEYNJ'),
(N'Character', N'চরিত্র, স্বভাব', N'Noun', N'Build strong character.', N'KAR-ik-ter'),
(N'Choose', N'বেছে নেওয়া, নির্বাচন করা', N'Verb', N'Choose your friends wisely.', N'CHOOZ'),
(N'Clear', N'স্পষ্ট, পরিষ্কার', N'Adjective', N'Make your intention clear.', N'KLEER'),
(N'Clever', N'চতুর, বুদ্ধিমান', N'Adjective', N'He is a clever boy.', N'KLEV-er'),

-- D
(N'Daily', N'দৈনিক, প্রতিদিন', N'Adjective', N'Read the daily newspaper.', N'DEY-lee'),
(N'Decision', N'সিদ্ধান্ত, সংকল্প', N'Noun', N'Make wise decisions.', N'dih-SIZH-uhn'),
(N'Dedicated', N'নিবেদিত, নিষ্ঠাবান', N'Adjective', N'Be dedicated to your work.', N'DED-i-key-tid'),
(N'Deep', N'গভীর, গাঢ়', N'Adjective', N'Think deep before deciding.', N'DEEP'),
(N'Develop', N'উন্নয়ন করা, বিকশিত হওয়া', N'Verb', N'Develop good habits early.', N'dih-VEL-uhp'),
(N'Different', N'ভিন্ন, আলাদা', N'Adjective', N'Try a different approach.', N'DIF-er-uhnt'),
(N'Difficult', N'কঠিন, দুরূহ', N'Adjective', N'This is a difficult question.', N'DIF-i-kuhlt'),
(N'Diligent', N'পরিশ্রমী, নিষ্ঠাবান', N'Adjective', N'Be diligent in your studies.', N'DIL-i-juhnt'),
(N'Discover', N'আবিষ্কার করা, খুঁজে পাওয়া', N'Verb', N'Discover your hidden talents.', N'dih-SKUHV-er'),
(N'Dream', N'স্বপ্ন, আকাঙ্ক্ষা', N'Noun', N'Follow your dreams always.', N'DREEM'),

-- E
(N'Early', N'প্রাথমিক, আগে', N'Adjective', N'Come to school early.', N'UR-lee'),
(N'Easy', N'সহজ, সরল', N'Adjective', N'This task is very easy.', N'EE-zee'),
(N'Education', N'শিক্ষা, শিক্ষাদান', N'Noun', N'Education is the key to success.', N'ej-oo-KEY-shuhn'),
(N'Effective', N'কার্যকর, ফলপ্রসূ', N'Adjective', N'Use effective methods.', N'ih-FEK-tiv'),
(N'Efficient', N'দক্ষ, কর্মক্ষম', N'Adjective', N'Work in an efficient manner.', N'ih-FISH-uhnt'),
(N'Effort', N'প্রচেষ্টা, চেষ্টা', N'Noun', N'Success requires effort.', N'EF-ert'),
(N'Encourage', N'উৎসাহিত করা, প্রেরণা দেওয়া', N'Verb', N'Encourage others to succeed.', N'en-KUR-ij'),
(N'Energy', N'শক্তি, সামর্থ্য', N'Noun', N'Conserve renewable energy.', N'EN-er-jee'),
(N'Enjoy', N'উপভোগ করা, আনন্দ পাওয়া', N'Verb', N'Enjoy every moment of life.', N'en-JOI'),
(N'Enough', N'যথেষ্ট, পর্যাপ্ত', N'Adjective', N'This is enough for now.', N'ih-NUHF'),

-- F
(N'Fair', N'ন্যায্য, ন্যায়সঙ্গত', N'Adjective', N'Make a fair decision.', N'FAIR'),
(N'Faith', N'বিশ্বাস, আস্থা', N'Noun', N'Have faith in yourself.', N'FEYTH'),
(N'Famous', N'বিখ্যাত, প্রসিদ্ধ', N'Adjective', N'He is a famous writer.', N'FEY-muhs'),
(N'Fast', N'দ্রুত, তাড়াতাড়ি', N'Adjective', N'Run fast to win the race.', N'FAST'),
(N'Favorite', N'প্রিয়, পছন্দের', N'Adjective', N'This is my favorite book.', N'FEY-ver-it'),
(N'Fear', N'ভয়, আতঙ্ক', N'Noun', N'Overcome your fears.', N'FEER'),
(N'Feel', N'অনুভব করা, মনে হওয়া', N'Verb', N'Feel grateful for what you have.', N'FEEL'),
(N'Final', N'চূড়ান্ত, শেষ', N'Adjective', N'This is the final decision.', N'FAHY-nl'),
(N'Find', N'খুঁজে পাওয়া, আবিষ্কার করা', N'Verb', N'Find the solution to problems.', N'FAHYND'),
(N'Focus', N'মনোনিবেশ, মনোযোগ', N'Verb', N'Focus on your goals.', N'FOH-kuhs'),

-- G
(N'Gain', N'লাভ করা, অর্জন করা', N'Verb', N'Gain knowledge through reading.', N'GEYN'),
(N'General', N'সাধারণ, সার্বিক', N'Adjective', N'This is general knowledge.', N'JEN-er-uhl'),
(N'Generous', N'উদার, দানশীল', N'Adjective', N'Be generous with your time.', N'JEN-er-uhs'),
(N'Gentle', N'মৃদু, কোমল', N'Adjective', N'Speak in a gentle manner.', N'JEN-tl'),
(N'Gift', N'উপহার, দান', N'Noun', N'Give gifts to loved ones.', N'GIFT'),
(N'Give', N'দেওয়া, প্রদান করা', N'Verb', N'Give help to those in need.', N'GIV'),
(N'Goal', N'লক্ষ্য, উদ্দেশ্য', N'Noun', N'Set clear goals for success.', N'GOHL'),
(N'Good', N'ভালো, উত্তম', N'Adjective', N'Do good deeds always.', N'GOOD'),
(N'Grade', N'গ্রেড, শ্রেণী', N'Noun', N'Get good grades in exams.', N'GREYD'),
(N'Grateful', N'কৃতজ্ঞ, ধন্যবাদী', N'Adjective', N'Be grateful for what you have.', N'GREYT-fuhl'),

-- H
(N'Habit', N'অভ্যাস, স্বভাব', N'Noun', N'Develop good habits early.', N'HAB-it'),
(N'Happy', N'খুশি, আনন্দিত', N'Adjective', N'Stay happy always.', N'HAP-ee'),
(N'Hard', N'কঠিন, কঠোর', N'Adjective', N'Work hard to achieve success.', N'HAHRD'),
(N'Healthy', N'স্বাস্থ্যকর, সুস্থ', N'Adjective', N'Eat healthy food daily.', N'HEL-thee'),
(N'Help', N'সাহায্য, সহায়তা', N'Noun', N'Help others in need.', N'HELP'),
(N'High', N'উচ্চ, উন্নত', N'Adjective', N'Set high goals for yourself.', N'HAHY'),
(N'Honest', N'সৎ, সত্যবাদী', N'Adjective', N'Always be honest in dealings.', N'ON-ist'),
(N'Honor', N'সম্মান, মর্যাদা', N'Noun', N'Honor your commitments always.', N'ON-er'),
(N'Hope', N'আশা, প্রত্যাশা', N'Noun', N'Never lose hope in life.', N'HOHP'),
(N'Humble', N'বিনীত, নম্র', N'Adjective', N'Stay humble despite success.', N'HUHM-buhl'),

-- I
(N'Idea', N'ধারণা, ভাবনা', N'Noun', N'Share your innovative ideas.', N'ahy-DEE-uh'),
(N'Imagine', N'কল্পনা করা, ভাবা', N'Verb', N'Imagine your bright future.', N'ih-MAJ-in'),
(N'Important', N'গুরুত্বপূর্ণ, প্রয়োজনীয়', N'Adjective', N'This is an important message.', N'im-PAWR-tnt'),
(N'Improve', N'উন্নতি করা, সুধারা', N'Verb', N'Improve your skills daily.', N'im-PROOV'),
(N'Include', N'অন্তর্ভুক্ত করা, শামিল করা', N'Verb', N'Include everyone in activities.', N'in-KLOOD'),
(N'Increase', N'বৃদ্ধি করা, বাড়ানো', N'Verb', N'Increase your knowledge daily.', N'in-KREES'),
(N'Indeed', N'প্রকৃতপক্ষে, সত্যিই', N'Adverb', N'He is indeed a good person.', N'in-DEED'),
(N'Inspire', N'অনুপ্রাণিত করা, উদ্বুদ্ধ করা', N'Verb', N'Teachers inspire students.', N'in-SPAHYR'),
(N'Intelligent', N'বুদ্ধিমান, মেধাবী', N'Adjective', N'She is an intelligent girl.', N'in-TEL-i-juhnt'),
(N'Interest', N'আগ্রহ, আকর্ষণ', N'Noun', N'Show interest in learning.', N'IN-ter-ist'),

-- J
(N'Join', N'যুক্ত হওয়া, যোগ দেওয়া', N'Verb', N'Join hands for success.', N'JOIN'),
(N'Journey', N'যাত্রা, ভ্রমণ', N'Noun', N'Life is a long journey.', N'JUR-nee'),
(N'Joy', N'আনন্দ, খুশি', N'Noun', N'Find joy in small things.', N'JOI'),
(N'Judge', N'বিচার করা, মূল্যায়ন করা', N'Verb', N'Do not judge people quickly.', N'JUHJ'),
(N'Just', N'ন্যায্য, ঠিক', N'Adjective', N'Be just and fair always.', N'JUHST'),
(N'Justice', N'ন্যায়বিচার, ইনসাফ', N'Noun', N'Everyone deserves justice.', N'JUHS-tis'),

-- K
(N'Keep', N'রাখা, বজায় রাখা', N'Verb', N'Keep your promises always.', N'KEEP'),
(N'Kind', N'দয়ালু, সদয়', N'Adjective', N'Be kind to everyone.', N'KAHYND'),
(N'Kindness', N'দয়া, সদয়তা', N'Noun', N'Show kindness to everyone.', N'KAHYND-nis'),
(N'Know', N'জানা, জ্ঞাত হওয়া', N'Verb', N'Know your strengths well.', N'NOH'),
(N'Knowledge', N'জ্ঞান, বিদ্যা', N'Noun', N'Knowledge is power.', N'NOL-ij'),

-- L
(N'Language', N'ভাষা, বাক্য', N'Noun', N'Learn a new language.', N'LANG-gwij'),
(N'Large', N'বড়, বৃহৎ', N'Adjective', N'This is a large building.', N'LAHRJ'),
(N'Last', N'শেষ, চূড়ান্ত', N'Adjective', N'This is the last chance.', N'LAST'),
(N'Late', N'দেরি, বিলম্ব', N'Adjective', N'Do not come late to class.', N'LEYT'),
(N'Leader', N'নেতা, নেতৃত্ব', N'Noun', N'Be a good leader.', N'LEE-der'),
(N'Learn', N'শেখা, শিক্ষা গ্রহণ করা', N'Verb', N'Learn something new every day.', N'LURN'),
(N'Leave', N'ছেড়ে যাওয়া, বিদায় নেওয়া', N'Verb', N'Leave bad habits behind.', N'LEEV'),
(N'Less', N'কম, অল্প', N'Adjective', N'Eat less junk food.', N'LES'),
(N'Lesson', N'পাঠ, শিক্ষা', N'Noun', N'Learn a lesson from mistakes.', N'LES-uhn'),
(N'Life', N'জীবন, প্রাণ', N'Noun', N'Live your life happily.', N'LAHYF'),

-- M
(N'Main', N'প্রধান, মূল', N'Adjective', N'Focus on the main point.', N'MEYN'),
(N'Maintain', N'বজায় রাখা, রক্ষণাবেক্ষণ করা', N'Verb', N'Maintain discipline in life.', N'meyn-TEYN'),
(N'Major', N'প্রধান, গুরুত্বপূর্ণ', N'Adjective', N'This is a major issue.', N'MEY-jer'),
(N'Make', N'তৈরি করা, বানানো', N'Verb', N'Make the right decision.', N'MEYK'),
(N'Manage', N'পরিচালনা করা, সামলানো', N'Verb', N'Manage your time wisely.', N'MAN-ij'),
(N'Manner', N'পদ্ধতি, আচরণ', N'Noun', N'Speak in a polite manner.', N'MAN-er'),
(N'Mark', N'চিহ্ন, নম্বর', N'Noun', N'Get good marks in exams.', N'MAHRK'),
(N'Matter', N'বিষয়, গুরুত্ব', N'Noun', N'Every detail matters.', N'MAT-er'),
(N'Mean', N'অর্থ, মানে', N'Verb', N'What does this word mean?', N'MEEN'),
(N'Memory', N'স্মৃতি, স্মরণশক্তি', N'Noun', N'Cherish beautiful memories.', N'MEM-uh-ree'),

-- N
(N'Natural', N'প্রাকৃতিক, স্বাভাবিক', N'Adjective', N'Eat natural foods.', N'NACH-er-uhl'),
(N'Nature', N'প্রকৃতি, স্বভাব', N'Noun', N'Protect nature always.', N'NEY-cher'),
(N'Near', N'নিকটবর্তী, কাছে', N'Adjective', N'The school is near my house.', N'NEER'),
(N'Necessary', N'প্রয়োজনীয়, আবশ্যক', N'Adjective', N'Sleep is necessary for health.', N'NES-uh-ser-ee'),
(N'Need', N'প্রয়োজন, দরকার', N'Noun', N'Everyone needs love and care.', N'NEED'),
(N'Never', N'কখনও না, কদাপি না', N'Adverb', N'Never give up on dreams.', N'NEV-er'),
(N'New', N'নতুন, নব', N'Adjective', N'Try something new today.', N'NOO'),
(N'Next', N'পরবর্তী, আগামী', N'Adjective', N'See you next week.', N'NEKST'),
(N'Nice', N'সুন্দর, মনোরম', N'Adjective', N'Have a nice day.', N'NAHYS'),
(N'Noble', N'মহৎ, উদার', N'Adjective', N'Perform noble deeds.', N'NOH-buhl'),

-- O
(N'Object', N'বস্তু, উদ্দেশ্য', N'Noun', N'Focus on the main object.', N'OB-jikt'),
(N'Observe', N'পর্যবেক্ষণ করা, লক্ষ্য করা', N'Verb', N'Observe nature carefully.', N'uhb-ZURV'),
(N'Obtain', N'লাভ করা, পাওয়া', N'Verb', N'Obtain good results through effort.', N'uhb-TEYN'),
(N'Obvious', N'স্পষ্ট, সুস্পষ্ট', N'Adjective', N'The answer is obvious.', N'OB-vee-uhs'),
(N'Offer', N'প্রস্তাব, প্রদান করা', N'Verb', N'Offer help to those in need.', N'AW-fer'),
(N'Often', N'প্রায়ই, অনেকসময়', N'Adverb', N'He often visits his parents.', N'AW-fuhn'),
(N'Opinion', N'মতামত, অভিমত', N'Noun', N'Share your honest opinion.', N'uh-PIN-yuhn'),
(N'Opportunity', N'সুযোগ, সম্ভাবনা', N'Noun', N'Grab every opportunity.', N'op-er-TOO-ni-tee'),
(N'Order', N'আদেশ, ক্রম', N'Noun', N'Maintain order in class.', N'AWR-der'),
(N'Ordinary', N'সাধারণ, মামুলি', N'Adjective', N'He is an ordinary person.', N'AWR-dn-er-ee'),

-- P
(N'Particular', N'বিশেষ, নির্দিষ্ট', N'Adjective', N'Pay attention to particular details.', N'per-TIK-yuh-ler'),
(N'Pass', N'পাস করা, উত্তীর্ণ হওয়া', N'Verb', N'Study hard to pass exams.', N'PAS'),
(N'Patience', N'ধৈর্য, সহিষ্ণুতা', N'Noun', N'Have patience in difficult times.', N'PEY-shuhns'),
(N'Peace', N'শান্তি, সমাধান', N'Noun', N'Promote peace in society.', N'PEES'),
(N'Perfect', N'নিখুঁত, পূর্ণ', N'Adjective', N'Practice makes perfect.', N'PUR-fikt'),
(N'Perform', N'সম্পাদন করা, করা', N'Verb', N'Perform your duties well.', N'per-FAWRM'),
(N'Perhaps', N'সম্ভবত, হয়তো', N'Adverb', N'Perhaps we will meet tomorrow.', N'per-HAPS'),
(N'Plan', N'পরিকল্পনা, পরিকল্পিত করা', N'Noun', N'Make a good plan before acting.', N'PLAN'),
(N'Please', N'অনুগ্রহ করে, দয়া করে', N'Adverb', N'Please help me with this.', N'PLEEZ'),
(N'Popular', N'জনপ্রিয়, প্রচলিত', N'Adjective', N'Cricket is a popular sport.', N'POP-yuh-ler'),

-- Q
(N'Quality', N'গুণমান, মান', N'Noun', N'Focus on quality, not quantity.', N'KWOL-i-tee'),
(N'Question', N'প্রশ্ন, জিজ্ঞাসা', N'Noun', N'Ask questions if you do not understand.', N'KWES-chuhn'),
(N'Quick', N'দ্রুত, তাড়াতাড়ি', N'Adjective', N'Give a quick response.', N'KWIK'),
(N'Quiet', N'শান্ত, নিঃশব্দ', N'Adjective', N'Keep quiet during class.', N'KWAHY-it'),
(N'Quit', N'ছেড়ে দেওয়া, বন্ধ করা', N'Verb', N'Never quit trying.', N'KWIT'),

-- R
(N'Raise', N'উঠানো, বাড়ানো', N'Verb', N'Raise your hand to answer.', N'REYZ'),
(N'Reach', N'পৌঁছানো, অর্জন করা', N'Verb', N'Reach your goals step by step.', N'REECH'),
(N'Read', N'পড়া, পাঠ করা', N'Verb', N'Read books daily for knowledge.', N'REED'),
(N'Ready', N'প্রস্তুত, তৈরি', N'Adjective', N'Be ready for any challenge.', N'RED-ee'),
(N'Real', N'প্রকৃত, বাস্তব', N'Adjective', N'Tell me the real story.', N'REE-uhl'),
(N'Reason', N'কারণ, যুক্তি', N'Noun', N'There must be a reason.', N'REE-zuhn'),
(N'Receive', N'পাওয়া, গ্রহণ করা', N'Verb', N'Receive blessings from elders.', N'ri-SEEV'),
(N'Recent', N'সাম্প্রতিক, সদ্য', N'Adjective', N'Tell me about recent events.', N'REE-suhnt'),
(N'Regular', N'নিয়মিত, স্বাভাবিক', N'Adjective', N'Maintain regular attendance.', N'REG-yuh-ler'),
(N'Respect', N'সম্মান, শ্রদ্ধা', N'Noun', N'Treat everyone with respect.', N'ri-SPEKT'),

-- S
(N'Safe', N'নিরাপদ, সুরক্ষিত', N'Adjective', N'Stay safe always.', N'SEYF'),
(N'Save', N'বাঁচানো, সঞ্চয় করা', N'Verb', N'Save money for future.', N'SEYV'),
(N'Say', N'বলা, কথা বলা', N'Verb', N'Say the truth always.', N'SEY'),
(N'School', N'বিদ্যালয়, পাঠশালা', N'Noun', N'Go to school regularly.', N'SKOOL'),
(N'See', N'দেখা, লক্ষ্য করা', N'Verb', N'See the beauty in everything.', N'SEE'),
(N'Seem', N'মনে হওয়া, প্রতীয়মান হওয়া', N'Verb', N'You seem tired today.', N'SEEM'),
(N'Sense', N'অনুভূতি, বোধশক্তি', N'Noun', N'Use common sense always.', N'SENS'),
(N'Serious', N'গুরুতর, গম্ভীর', N'Adjective', N'Take your studies seriously.', N'SEER-ee-uhs'),
(N'Serve', N'সেবা করা, পরিবেশন করা', N'Verb', N'Serve your country with pride.', N'SURV'),
(N'Simple', N'সরল, সহজ', N'Adjective', N'Keep it simple and clear.', N'SIM-puhl'),

-- T
(N'Take', N'নেওয়া, গ্রহণ করা', N'Verb', N'Take responsibility for actions.', N'TEYK'),
(N'Talk', N'কথা বলা, আলাপ করা', N'Verb', N'Talk politely with everyone.', N'TAWK'),
(N'Task', N'কাজ, দায়িত্ব', N'Noun', N'Complete each task properly.', N'TASK'),
(N'Teach', N'শেখানো, শিক্ষা দেওয়া', N'Verb', N'Teachers teach with dedication.', N'TEECH'),
(N'Team', N'দল, টিম', N'Noun', N'Work together as a team.', N'TEEM'),
(N'Tell', N'বলা, জানানো', N'Verb', N'Tell the truth always.', N'TEL'),
(N'Thank', N'ধন্যবাদ দেওয়া, কৃতজ্ঞতা প্রকাশ', N'Verb', N'Thank people who help you.', N'THANGK'),
(N'Think', N'চিন্তা করা, ভাবা', N'Verb', N'Think before you speak.', N'THINGK'),
(N'Time', N'সময়, কাল', N'Noun', N'Time is very precious.', N'TAHYM'),
(N'Today', N'আজ, আজকের দিন', N'Noun', N'Today is a new opportunity.', N'tuh-DEY'),

-- U
(N'Understand', N'বুঝা, উপলব্ধি করা', N'Verb', N'Understand before you judge.', N'uhn-der-STAND'),
(N'Unique', N'অনন্য, অদ্বিতীয়', N'Adjective', N'Everyone is unique in their way.', N'yoo-NEEK'),
(N'Unity', N'ঐক্য, একতা', N'Noun', N'Unity is strength.', N'YOO-ni-tee'),
(N'Universal', N'সার্বজনীন, সর্বজনীন', N'Adjective', N'Love is a universal language.', N'yoo-nuh-VUR-suhl'),
(N'Useful', N'উপকারী, দরকারী', N'Adjective', N'This tool is very useful.', N'YOOS-fuhl'),

-- V
(N'Value', N'মূল্য, গুরুত্ব', N'Noun', N'Understand the value of time.', N'VAL-yoo'),
(N'Victory', N'বিজয়, জয়', N'Noun', N'Celebrate your victory humbly.', N'VIK-tuh-ree'),
(N'View', N'দৃষ্টিভঙ্গি, দৃশ্য', N'Noun', N'Respect different points of view.', N'VYOO'),
(N'Visit', N'পরিদর্শন, ভ্রমণ', N'Verb', N'Visit your grandparents often.', N'VIZ-it'),
(N'Voice', N'কণ্ঠস্বর, মত', N'Noun', N'Raise your voice against injustice.', N'VOIS'),

-- W
(N'Wait', N'অপেক্ষা করা, প্রতীক্ষা করা', N'Verb', N'Wait for the right time.', N'WEYT'),
(N'Walk', N'হাঁটা, পদচারণা করা', N'Verb', N'Walk regularly for health.', N'WAWK'),
(N'Want', N'চাওয়া, ইচ্ছা করা', N'Verb', N'Work hard for what you want.', N'WONT'),
(N'Watch', N'দেখা, পর্যবেক্ষণ করা', N'Verb', N'Watch and learn from others.', N'WOCH'),
(N'Welcome', N'স্বাগত, স্বাগতম', N'Verb', N'Welcome guests warmly.', N'WEL-kuhm'),
(N'Win', N'জয়ী হওয়া, জেতা', N'Verb', N'Win with grace and humility.', N'WIN'),
(N'Wisdom', N'প্রজ্ঞা, বুদ্ধিমত্তা', N'Noun', N'Seek wisdom through learning.', N'WIZ-duhm'),
(N'Wise', N'জ্ঞানী, বিচক্ষণ', N'Adjective', N'Make wise decisions in life.', N'WAHYZ'),
(N'Wish', N'ইচ্ছা, কামনা', N'Noun', N'Your wish will come true.', N'WISH'),
(N'Work', N'কাজ, শ্রম', N'Noun', N'Hard work leads to success.', N'WURK'),

-- Y
(N'Year', N'বছর, বর্ষ', N'Noun', N'This year will be great.', N'YEER'),
(N'Yes', N'হ্যাঁ, সম্মতিসূচক', N'Interjection', N'Yes, I agree with you.', N'YES'),
(N'Yesterday', N'গতকাল, গত দিন', N'Noun', N'Yesterday was a good day.', N'YES-ter-dey'),
(N'Young', N'তরুণ, যুব', N'Adjective', N'Stay young at heart.', N'YUHNG'),
(N'Youth', N'যুবক, তারুণ্য', N'Noun', N'Youth is the future.', N'YOOTH');

PRINT 'Sample words inserted successfully!'
PRINT 'Total: 200 words'
GO

-- =====================================================
-- Step 3: Verify Installation
-- =====================================================
PRINT ''
PRINT '=== VERIFICATION ==='
GO

SELECT COUNT(*) AS TotalWords FROM WordOfTheDay;

SELECT TOP 10 
    WordID,
    EnglishWord,
    BengaliMeaning,
    PartOfSpeech
FROM WordOfTheDay
ORDER BY WordID;

PRINT ''
PRINT '=== INSTALLATION COMPLETED ==='
PRINT 'Table: WordOfTheDay (Simplified - No Category)'
PRINT 'Format: 5 columns only'
PRINT 'Sample: 200 words inserted'
PRINT ''
PRINT 'INSERT Format:'
PRINT 'INSERT INTO [dbo].[WordOfTheDay]'
PRINT '(EnglishWord, BengaliMeaning, PartOfSpeech, ExampleSentence, Pronunciation)'
PRINT 'VALUES'
PRINT '(N''Ability'', N''ক্ষমতা, সামর্থ্য'', N''Noun'', N''She has the ability to lead.'', N''uh-BIL-i-tee'')'
PRINT ''
GO

using System;
using System.Media;
using System.Text.RegularExpressions;

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace CyberSecurityChatBot
{
    class Program
    {
        // Remember the user’s favorite part of learning cybersecurity
        static string favoritePart = null;

        //Tracks the most recently discussed topic
        static string lastTopic = null;

        static readonly Random rnd = new Random();

        //Task management
        class TaskItem
        {
            public string Title;
            public string Description;
            public DateTime? ReminderDate;
            public bool Reminded = false;
        }

        static readonly List<TaskItem> tasksList = new List<TaskItem>();
        static System.Timers.Timer reminderTimer;

        //Topic-specific response lists
        static readonly ArrayList passwordTips = new ArrayList
        {
            "Use a mix of upper-/lower-case letters, numbers, and symbols in every password.",
            "Consider a reputable password manager to generate and store long, unique passwords.",
            "Change critical account passwords regularly and never reuse them across sites."
        };

        static readonly ArrayList scamTips = new ArrayList
        {
            "Scam Alert: Always verify the sender’s identity before clicking unknown links.",
            "If something sounds too good to be true, it probably is—don’t share personal info.",
            "Hover over links to check their real destination, and never download unexpected attachments."
        };

        static readonly ArrayList privacyTips = new ArrayList
        {
            "Limit sharing personal details online and review app privacy settings routinely.",
            "Use end-to-end encrypted messaging apps (e.g., Signal) for sensitive conversations.",
            "Consider a VPN or browser privacy extensions to minimize tracking."
        };

        static readonly ArrayList safetyTips = new ArrayList
        {
            "Keep your OS and applications up to date to patch known vulnerabilities.",
            "Only browse HTTPS sites and consider reputable ad-blockers to reduce risk.",
            "Run antivirus and enable your firewall to block malicious downloads."
        };

        static void Main(string[] args)
        {
            //Start reminder checker
            reminderTimer = new System.Timers.Timer(20_000);
            reminderTimer.Elapsed += (s, e) => CheckReminders();
            reminderTimer.AutoReset = true;
            reminderTimer.Start();

            

            //Console UI
            Console.Title = "BotSafe Cybersecurity Assistant";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("********************");
            Console.WriteLine("***Welcome to BotSafe, your cyber security assistant!***");
            Console.WriteLine("********************");
            Console.ResetColor();
            Console.WriteLine(@"""
 ____        _   ____         __      
| __ )  ___ | |_/ ___|  __ _ / _| ___ 
|  _ \ / _ \| __\___ \ / _ | |_ / _ \
| |_) | (_) | |_ ___) | (_| |  _|  __/
|____/ \___/ \__|____/ \__,_|_|  \___|                          

                            ----.,
                           |  o_o|,
                           |:_/  |,
                          //   \\ \\,
                         (|     | ),
                        /'\\_   _/\\,
                        \\___)=(___/"");");

            Console.WriteLine("\nHow can I help you today? (Type 'exit' to quit application)\n");
            Console.WriteLine("Let's get to know each other before we begin.\n");
            Thread.Sleep(500);

            string firstName = PromptForInput("Please enter your first name: ", @"^[A-Za-z]+$", "First name should only contain letters.");
            string surname = PromptForInput("Please enter your surname: ", @"^[A-Za-z]+$", "Surname should only contain letters.");
            int age = PromptForAge("Please enter your age: ");

            Console.WriteLine($"\nNice to meet you, {firstName} {surname}!");
            if (age < 18)
                Console.WriteLine("I see you're still young! Remember, it's never too early to learn about cyber security.");
            else if (age < 35)
                Console.WriteLine("It's great to see adults taking an interest in cyber security. Let's grow your knowledge!");
            else
                Console.WriteLine("Let's make sure you're up to date with the latest in cyber security!");
            Thread.Sleep(500);

            Console.Write("\nWhat’s your favorite part of learning cybersecurity? ");
            favoritePart = Console.ReadLine()?.Trim();
            while (string.IsNullOrWhiteSpace(favoritePart))
            {
                Console.Write("Please share at least a few words about your favorite aspect: ");
                favoritePart = Console.ReadLine()?.Trim();
            }
            Console.WriteLine($"\nGot it—so you love {favoritePart}! Let’s talk more about that as we go.\n");
            Thread.Sleep(500);

            PrintSectionHeader("CHAT SESSION");
            Console.WriteLine("\nHow can I assist you today? (Commands: add task, view tasks, start quiz, exit)");

            while (true)
            {
                Console.Write("\n> ");
                string userInput = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine("Please enter a valid input."); continue;
                }

                if (userInput.StartsWith("add task", StringComparison.OrdinalIgnoreCase) ||
                    userInput.StartsWith("remind me", StringComparison.OrdinalIgnoreCase))
                {
                    AddTaskFlow(); continue;
                }

                if (userInput.Equals("view tasks", StringComparison.OrdinalIgnoreCase))
                {
                    ViewTasksFlow(); continue;
                }

                if (userInput.Equals("start quiz", StringComparison.OrdinalIgnoreCase))
                {
                    StartQuizFlow(); continue;
                }

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"\nGoodbye, {firstName} {surname}! Stay safe online."); break;
                }

                Thread.Sleep(500);
                Console.WriteLine(GetResponse(userInput));
            }
        }

        // Quiz Flow
        static void StartQuizFlow()
        {
            var questions = new List<(string, string[], int)>
            {
                ("Phishing is a form of social engineering. True or False?", new[]{"True","False"}, 0),
                ("A strong password should be at least how many characters? A)6 B)8 C)12 D)4", new[]{"A","B","C","D"}, 2),
                ("You should share passwords with coworkers. True or False?", new[]{"True","False"}, 1),
                ("Which is safest? A)Clicking unsolicited links B)Verifying sender before clicking", new[]{"A","B"}, 1),
                ("Two-factor authentication is optional but increases security. True or False?", new[]{"True","False"}, 0),
                ("Use the same password on multiple sites. True or False?", new[]{"True","False"}, 1),
                ("A VPN helps protect your: A)Passwords B)Data on public Wi-Fi C)Keyboard strength", new[]{"A","B","C"}, 1),
                ("Social engineering attacks exploit: A)Software bugs B)Human trust C)Hardware failure", new[]{"A","B","C"}, 1),
                ("Safe browsing means looking for HTTPS in the URL. True or False?", new[]{"True","False"}, 0),
                ("You get an email from IT asking your password to 'fix issues.' True or False?", new[]{"True","False"}, 1)
            };
            int score = 0;
            Console.WriteLine("\n🔐 Starting Cybersecurity Quiz!\n");
            for (int i = 0; i < questions.Count; i++)
            {
                var (Q, Opts, ans) = questions[i]; Console.WriteLine($"{i + 1}. {Q}");
                if (Opts.Length > 2) { char l = 'A'; foreach (var o in Opts) Console.Write($" {l++}){o}"); Console.WriteLine(); }
                else Console.WriteLine(" True  False");
                Console.Write("> "); string r = Console.ReadLine()?.Trim(); bool correct = false;
                if (Opts.Length == 2)
                    correct = (ans == 0 && r.Equals("true", StringComparison.OrdinalIgnoreCase)) || (ans == 1 && r.Equals("false", StringComparison.OrdinalIgnoreCase));
                else correct = (r.Length > 0 && (r.ToUpper()[0] - 'A') == ans);
                if (correct) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("✅ Correct!\n"); score++; }
                else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("❌ Incorrect.\n"); }
                Console.ResetColor(); Thread.Sleep(400);
            }
            int wrong = questions.Count - score;
            Console.WriteLine($"🎯 Quiz complete! You scored {score}/{questions.Count}.\n");
            if (wrong < 5) Console.WriteLine("👏 Great job! Keep up the good work learning cybersecurity.");
            if (score >= 5) Console.WriteLine("🏆 Well done! You are already a pro!");
            else if (score < 5) Console.WriteLine("👍 Not bad—review the tips and try again to improve!");
            Console.WriteLine();
        }

        static void AddTaskFlow()
        {
            Console.WriteLine("\n📝 Let's add a new cybersecurity task.");
            Console.Write("Title: "); string title = Console.ReadLine()?.Trim() ?? "";
            while (string.IsNullOrWhiteSpace(title)) { Console.Write("Please enter a non-empty title: "); title = Console.ReadLine()?.Trim() ?? ""; }
            Console.Write("Description: "); string desc = Console.ReadLine()?.Trim() ?? "";
            while (string.IsNullOrWhiteSpace(desc)) { Console.Write("Please enter a non-empty description: "); desc = Console.ReadLine()?.Trim() ?? ""; }
            DateTime? d = null; Console.Write("Would you like a reminder? (yes/no): "); var rsp = Console.ReadLine()?.Trim().ToLower();
            if (rsp == "yes" || rsp == "y")
            {
                Console.Write("Enter reminder timeframe (e.g. 'in 3 days' or '2025-07-01'): "); var w = Console.ReadLine()?.Trim().ToLower() ?? ""; var m = Regex.Match(w, @"in\s+(\d+)\s+days");
                if (m.Success && int.TryParse(m.Groups[1].Value, out int dy)) d = DateTime.Now.AddDays(dy);
                else if (DateTime.TryParse(w, out DateTime dt)) d = dt;
            }
            tasksList.Add(new TaskItem { Title = title, Description = desc, ReminderDate = d });
            Console.WriteLine($"\n✅ Task '{title}' added{(d.HasValue ? $" with reminder on {d:yyyy-MM-dd}" : "")}.");
        }

        static void ViewTasksFlow()
        {
            Console.WriteLine("\n🗒️ Your current cybersecurity tasks:");
            if (tasksList.Count == 0) { Console.WriteLine("   (No tasks have been added yet.)"); return; }
            for (int i = 0; i < tasksList.Count; i++)
            {
                var t = tasksList[i]; string st = t.Reminded ? "✅ Completed" : "🔲 Pending";
                string du = t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString("yyyy-MM-dd HH:mm") : "—";
                Console.WriteLine($" {i + 1}. [{st}] {t.Title} (due: {du})"); Console.WriteLine($"      {t.Description}");
            }
            Console.WriteLine("\nOptions: [D]elete <#>, [B]ack"); Console.Write("Choice: "); var c = Console.ReadLine()?.Trim().ToUpper();
            if (string.IsNullOrEmpty(c) || c == "B") return; var p = c.Split(' ');
            if (p.Length == 2 && p[0] == "D" && int.TryParse(p[1], out int idx) && idx >= 1 && idx <= tasksList.Count) DeleteTask(idx - 1);
            else Console.WriteLine("Invalid option. Returning to chat.");
        }

        static void DeleteTask(int i)
        {
            var t = tasksList[i].Title; tasksList.RemoveAt(i);
            Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"\n🗑️ Deleted task '{t}'."); Console.ResetColor();
        }

        static void CheckReminders()
        {
            var n = DateTime.Now; foreach (var t in tasksList) if (t.ReminderDate.HasValue && !t.Reminded && t.ReminderDate.Value <= n)
                { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"\n🔔 REMINDER: '{t.Title}' — {t.Description}"); Console.ResetColor(); t.Reminded = true; }
        }

        static string PromptForInput(string pr, string pat, string err) { while (true) { Console.Write(pr); var i = Console.ReadLine(); if (Regex.IsMatch(i, pat)) return i; Console.WriteLine(err); } }
        static int PromptForAge(string pr) { while (true) { Console.Write(pr); if (int.TryParse(Console.ReadLine(), out int a) && a > 0) return a; Console.WriteLine("Please enter a valid age"); } }

        static string GetResponse(string input)
        {
            string lower = input.ToLower().Trim(); bool IsFollowUp() => lower.Contains("more") || lower.Contains("detail") || lower.Contains("explain") || lower.Contains("confused") || lower.Contains("again") || lower.Contains("elaborate") || lower.Contains("further");
            if (IsFollowUp() && lastTopic != null)
            {
                switch (lastTopic) { case "passwords": return $"{passwordTips[rnd.Next(passwordTips.Count)]} Since you love {favoritePart}, try a password manager!"; case "scam": return (string)scamTips[rnd.Next(scamTips.Count)]; case "privacy": return $"{privacyTips[rnd.Next(privacyTips.Count)]} Since you love {favoritePart}, check privacy tools!"; case "safety": return (string)safetyTips[rnd.Next(safetyTips.Count)]; }
                var s = DetectSentiment(lower); if (s != null) { switch (s) { case "worried": return "I understand your concerns—let's work through this together."; case "curious": return "Curiosity sparks learning! Ask anything."; case "frustrated": return "Frustration is natural—let's break it down."; case "help": return "I’m here to help—what do you need?"; } }
            }
            if (lower.Contains("password")) { lastTopic = "passwords"; return (string)passwordTips[rnd.Next(passwordTips.Count)]; }
            if (lower.Contains("scam")) { lastTopic = "scam"; return (string)scamTips[rnd.Next(scamTips.Count)]; }
            if (lower.Contains("privacy")) { lastTopic = "privacy"; return (string)privacyTips[rnd.Next(privacyTips.Count)]; }
            if (lower.Contains("safety")) { lastTopic = "safety"; return (string)safetyTips[rnd.Next(safetyTips.Count)]; }
            lastTopic = null; if (lower.Contains("attack")) return "Caution: Cyber attacks are serious. Stay safe!"; return "I don't really understand that. Could you rephrase?";
        }

        static string DetectSentiment(string txt) { if (txt.Contains("worried") || txt.Contains("anxious")) return "worried"; if (txt.Contains("curious") || txt.Contains("wondering")) return "curious"; if (txt.Contains("frustrated") || txt.Contains("stuck")) return "frustrated"; if (txt.Contains("help") || txt.Contains("assist")) return "help"; return null; }
        static void PrintSectionHeader(string t) { Console.WriteLine(new string('=', 50)); Console.WriteLine($"| {t.PadRight(46)}|"); Console.WriteLine(new string('=', 50)); }
    }
}

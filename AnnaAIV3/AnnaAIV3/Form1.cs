using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using AIMLbot;
using System.Diagnostics;

namespace AnnaAIV3
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine recognition;
        private SpeechSynthesizer synth;

        private Bot bot;
        private User user;

        private Dictionary<string, string> commands = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech();
        }

        private void LoadSpeech()
        {
            try
            {
                #region Speech Stuff
                recognition = new SpeechRecognitionEngine();
                recognition.SetInputToDefaultAudioDevice();
                recognition.LoadGrammar(new DictationGrammar());

                recognition.RecognizeAsync(RecognizeMode.Multiple);

                bot = new Bot();
                bot.loadSettings();

                user = new User("Gianna", bot);

                bot.isAcceptingUserInput = false;
                bot.loadAIMLFromFiles();
                bot.isAcceptingUserInput = true;

                synth = new SpeechSynthesizer();

                synth.SelectVoiceByHints(VoiceGender.Female);

                recognition.SpeechRecognized += OnSpeechRecognized;
                #endregion

                #region Commands

                commands.Add("what time is it", "WhatTime");
                commands.Add("what is the time", "WhatTime");
                commands.Add("can you tell me the time", "WhatTime");
                commands.Add("can you tell me the time please", "WhatTime");

                commands.Add("can you tell me the date please", "WhatDate");
                commands.Add("can you tell me the date", "WhatDate");
                commands.Add("what is the date", "WhatDate");
                commands.Add("what is the date today", "WhatDate");
                commands.Add("what day is it", "WhatDate");
                commands.Add("what is the day today", "WhatDate");

                commands.Add("exit", "exit");

                string[] commandsArrary = commands.Keys.ToArray();
                Choices commandsChoices = new Choices(commandsArrary);
                GrammarBuilder builder = new GrammarBuilder();
                builder.Append(commandsChoices);
                Grammar grammar = new Grammar(builder);
                grammar.Name = "Commands";

                recognition.LoadGrammar(grammar);

                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string s = e.Result.Text;

            if (e.Result.Confidence > 0.4)
            {
                switch (e.Result.Grammar.Name)
                {
                    case ("Commands"):
                        
                        try
                        {
                            string commandType = commands[s];
                            synth.SpeakAsync(ProcessCommand(commandType));
                        }
                        catch
                        {
                            break;
                        }

                        break;

                    default:
                        if(s.StartsWith("google") || s.StartsWith("Google"))
                        {
                            string args = s.Substring(5);

                            Process.Start($"https://www.google.com/search?q=" + args);
                        }

                        synth.SpeakAsync(GetResponse(s));
                        break;
                }

                label1.Text = s;
                // synth.SpeakAsync(GetResponse(s));
            }
        }

        private string ProcessCommand(string type)
        {
            string answer = "Sorry, there was an error";

            switch(type)
            {
                case ("WhatTime"):
                    answer = $"It is {DateTime.Now.ToShortTimeString()}";
                    break;

                case ("WhatDate"):
                    answer = $"It is {DateTime.Now.ToLongDateString()}";
                    break;

                case ("exit"):
                    Application.Exit();
                    break;
            }

            return answer;
        }

        private string GetResponse(string input)
        {
            Request request = new Request(input, user, bot);
            Result result = bot.Chat(request);
            return result.Output;
        }
    }
}
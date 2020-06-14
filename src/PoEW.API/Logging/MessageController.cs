using PoEW.API.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoEW.API.Logging {
    public class MessageController {
        #region Singleton
        private static MessageController _instance;
        public static MessageController Instance() {
            if (_instance == null) {
                _instance = new MessageController();
            }

            return _instance;
        }
        #endregion

        Dictionary<ILogger, Queue<string>> Loggers = new Dictionary<ILogger, Queue<string>>();

        Task LogTask;

        Queue<string> Messages = new Queue<string>();
        int MaxMessagesCount = 10;

        private MessageController() {
            LogTask = new Task(() => {
                while (true) {
                    foreach (var logger in Loggers) {
                        try {
                            logger.Key.Log(logger.Value.Dequeue());
                        } catch (InvalidOperationException e) {
                        }
                    }

                    Thread.Sleep(200);
                }
            });
            LogTask.Start();
        }

        public void AddLogger(ILogger logger) {
            Loggers.Add(logger, Utils.DeepClone(Messages));
        }

        private string TimeToString() {
            return DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
        }

        public void Log(string message, object obj = null) {
            Messages.Enqueue($"{TimeToString()} {message} {obj}");

            if (Messages.Count > MaxMessagesCount) {
                Messages.Dequeue();
            }

            foreach (var logger in Loggers) {
                logger.Value.Enqueue($"{TimeToString()} {message} {obj}");
            }
        }

        public void Log(object obj) {
            Messages.Enqueue($"{TimeToString()} {obj}");

            if (Messages.Count > MaxMessagesCount) {
                Messages.Dequeue();
            }

            foreach (var logger in Loggers) {
                logger.Value.Enqueue($"{TimeToString()} {obj}");
            }
        }
    }
}

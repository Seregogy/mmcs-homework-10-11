using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Homework10
{
    // Класс, представляющий математическую формулу
    public class Formula
    {
        public string Topic { get; }
        public string Name { get; }
        public string Expression { get; }

        public Formula(string topic, string name, string expression)
        {
            Topic = topic;
            Name = name;
            Expression = expression;
        }

        public override string ToString()
        {
            return $"{Topic}: {Name} = {Expression}";
        }
    }

    // Класс для хранения статистики по ответам пользователя
    public class AnswerStatistic
    {
        public Formula Formula { get; }
        public int CorrectAnswers { get; private set; }
        public int IncorrectAnswers { get; private set; }

        public AnswerStatistic(Formula formula)
        {
            Formula = formula;
        }

        public void AddCorrectAnswer()
        {
            CorrectAnswers++;
        }

        public void AddIncorrectAnswer()
        {
            IncorrectAnswers++;
        }

        public double SuccessRate =>
            CorrectAnswers + IncorrectAnswers == 0 ? 0 :
            (double)CorrectAnswers / (CorrectAnswers + IncorrectAnswers) * 100;
    }

    // Класс тренажера формул
    public class FormulaTrainer
    {
        private readonly Dictionary<string, List<Formula>> _formulasByTopic = new();
        private readonly List<AnswerStatistic> _statistics = new();
        private readonly Random _random = new();

        // Загрузка формул из файла
        public void LoadFormulasFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл с формулами не найден.");
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('|');
                if (parts.Length != 3)
                {
                    Console.WriteLine($"Ошибка формата в строке: {line}");
                    continue;
                }

                var topic = parts[0].Trim();
                var name = parts[1].Trim();
                var expression = parts[2].Trim();

                AddFormula(new Formula(topic, name, expression));
            }
        }

        // Добавление формулы в тренажер
        public void AddFormula(Formula formula)
        {
            if (!_formulasByTopic.ContainsKey(formula.Topic))
            {
                _formulasByTopic[formula.Topic] = new List<Formula>();
            }

            if (!_formulasByTopic[formula.Topic].Any(f => f.Name == formula.Name))
            {
                _formulasByTopic[formula.Topic].Add(formula);
                _statistics.Add(new AnswerStatistic(formula));
            }
        }

        // Получение списка тем
        public List<string> GetTopics()
        {
            return _formulasByTopic.Keys.ToList();
        }

        // Получение формул по теме
        public List<Formula> GetFormulasByTopic(string topic)
        {
            return _formulasByTopic.ContainsKey(topic) ? _formulasByTopic[topic] : new List<Formula>();
        }

        // Проведение тренировки
        public void ConductTraining(List<string> selectedTopics)
        {
            if (selectedTopics == null || selectedTopics.Count == 0)
            {
                Console.WriteLine("Не выбрано ни одной темы для тренировки.");
                return;
            }

            var availableFormulas = selectedTopics
                .Where(topic => _formulasByTopic.ContainsKey(topic))
                .SelectMany(topic => _formulasByTopic[topic])
                .ToList();

            if (availableFormulas.Count == 0)
            {
                Console.WriteLine("По выбранным темам не найдено формул.");
                return;
            }

            // Учитываем статистику для выбора формул
            var weightedFormulas = availableFormulas
                .Select(formula =>
                {
                    var stat = _statistics.FirstOrDefault(s => s.Formula == formula);
                    var incorrectCount = stat?.IncorrectAnswers ?? 0;
                    // Чем больше неправильных ответов, тем чаще показываем формулу
                    return Enumerable.Repeat(formula, incorrectCount + 1);
                })
                .SelectMany(x => x)
                .ToList();

            Console.WriteLine("\nТренировка началась! Нажмите Enter, чтобы увидеть правильный ответ.");
            Console.WriteLine("После просмотра ответа введите '+', если вспомнили правильно, или '-', если ошиблись.");
            Console.WriteLine("Для выхода из тренировки введите 'выход'.\n");

            while (true)
            {
                // Выбираем случайную формулу с учетом весов
                var randomIndex = _random.Next(weightedFormulas.Count);
                var currentFormula = weightedFormulas[randomIndex];
                var currentStat = _statistics.First(s => s.Formula == currentFormula);

                Console.WriteLine($"Формула: {currentFormula.Name}");
                Console.Write("Нажмите Enter чтобы увидеть ответ...");
                var input = Console.ReadLine();

                if (input?.Trim().ToLower() == "выход")
                    break;

                Console.WriteLine($"Правильный ответ: {currentFormula.Expression}");

                bool? isCorrect = null;
                while (isCorrect == null)
                {
                    Console.Write("Вы вспомнили правильно (+/-)? ");
                    input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "+")
                    {
                        isCorrect = true;
                        currentStat.AddCorrectAnswer();
                        Console.WriteLine("Верно! Отлично!\n");
                    }
                    else if (input == "-")
                    {
                        isCorrect = false;
                        currentStat.AddIncorrectAnswer();
                        Console.WriteLine("Ошибка. Постарайтесь запомнить эту формулу.\n");
                    }
                    else if (input == "выход")
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Пожалуйста, введите '+' или '-'.");
                    }
                }
            }
        }

        // Вывод статистики
        public void PrintStatistics(int? lastTrainingsCount = null)
        {
            if (_statistics.Count == 0)
            {
                Console.WriteLine("Статистика пока отсутствует.");
                return;
            }

            Console.WriteLine("\nСтатистика неправильных ответов:");

            // Группируем по темам
            var topicsStats = _statistics
                .GroupBy(s => s.Formula.Topic)
                .OrderByDescending(g => g.Sum(s => s.IncorrectAnswers));

            foreach (var topicGroup in topicsStats)
            {
                Console.WriteLine($"\nТема: {topicGroup.Key}");
                Console.WriteLine($"Всего ошибок: {topicGroup.Sum(s => s.IncorrectAnswers)}");

                var formulasInTopic = topicGroup
                    .OrderByDescending(s => s.IncorrectAnswers)
                    .ThenBy(s => s.Formula.Name);

                foreach (var stat in formulasInTopic)
                {
                    if (stat.IncorrectAnswers > 0)
                    {
                        Console.WriteLine($"- {stat.Formula.Name}: {stat.IncorrectAnswers} ошибок " +
                                        $"(успешность: {stat.SuccessRate:F1}%)");
                    }
                }
            }

            var totalCorrect = _statistics.Sum(s => s.CorrectAnswers);
            var totalIncorrect = _statistics.Sum(s => s.IncorrectAnswers);
            var total = totalCorrect + totalIncorrect;
            var successRate = total == 0 ? 0 : (double)totalCorrect / total * 100;

            Console.WriteLine($"\nОбщая статистика: {totalCorrect} правильных, " +
                            $"{totalIncorrect} неправильных ответов " +
                            $"(успешность: {successRate:F1}%)");
        }
    }
}
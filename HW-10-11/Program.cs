namespace Homework10
{
    class Program
    {
        static void Main(string[] args)
        {
            var trainer = new FormulaTrainer();

            try
            {
                trainer.LoadFormulasFromFile("formulas.txt");
                Console.WriteLine("Формулы успешно загружены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки формул: {ex.Message}");
                Console.WriteLine("Продолжение работы без формул невозможно.");
                return;
            }

            while (true)
            {
                Console.WriteLine("\nТренажёр математики");
                Console.WriteLine("1. Начать тренировку");
                Console.WriteLine("2. Просмотреть список тем и формул");
                Console.WriteLine("3. Просмотреть статистику");
                Console.WriteLine("4. Выход");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        StartTraining(trainer);
                        break;
                    case "2":
                        ShowTopicsAndFormulas(trainer);
                        break;
                    case "3":
                        ShowStatistics(trainer);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте еще раз.");
                        break;
                }
            }
        }

        static void StartTraining(FormulaTrainer trainer)
        {
            var topics = trainer.GetTopics();
            if (topics.Count == 0)
            {
                Console.WriteLine("Нет доступных тем для тренировки.");
                return;
            }

            Console.WriteLine("\nДоступные темы:");
            for (int i = 0; i < topics.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {topics[i]}");
            }

            Console.WriteLine("\nВведите номера тем для тренировки через пробел (например: 1 2)");
            Console.Write("Или введите 'все' для выбора всех тем: ");
            var input = Console.ReadLine();

            List<string> selectedTopics;
            if (input?.Trim().ToLower() == "все")
            {
                selectedTopics = topics;
            }
            else
            {
                var selectedIndices = input?
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var num) ? num : -1)
                    .Where(num => num > 0 && num <= topics.Count)
                    .Select(num => topics[num - 1])
                    .ToList();

                if (selectedIndices == null || selectedIndices.Count == 0)
                {
                    Console.WriteLine("Не выбрано ни одной темы. Тренировка отменена.");
                    return;
                }

                selectedTopics = selectedIndices;
            }

            trainer.ConductTraining(selectedTopics);
        }

        static void ShowTopicsAndFormulas(FormulaTrainer trainer)
        {
            var topics = trainer.GetTopics();
            if (topics.Count == 0)
            {
                Console.WriteLine("Нет доступных тем.");
                return;
            }

            Console.WriteLine("\nСписок тем и формул:");
            foreach (var topic in topics)
            {
                Console.WriteLine($"\nТема: {topic}");
                var formulas = trainer.GetFormulasByTopic(topic);
                foreach (var formula in formulas)
                {
                    Console.WriteLine($"- {formula.Name}: {formula.Expression}");
                }
            }
        }

        static void ShowStatistics(FormulaTrainer trainer)
        {
            Console.Write("\nВведите количество последних тренировок для анализа (оставьте пустым для всей статистики): ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                trainer.PrintStatistics();
            }
            else if (int.TryParse(input, out var count) && count > 0)
            {
                trainer.PrintStatistics(count);
            }
            else
            {
                Console.WriteLine("Некорректный ввод. Будет показана вся статистика.");
                trainer.PrintStatistics();
            }
        }
    }
}

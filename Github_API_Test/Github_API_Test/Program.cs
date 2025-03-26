using System;

namespace Github_API_Test
{
	internal class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.WriteLine("Введите end для выхода из программы");
				Console.WriteLine($"1. Список проблем для репозитория {GithubRequest.Repos}");
				Console.WriteLine($"2. Создание проблемы для репозитория {GithubRequest.Repos}");
				Console.WriteLine($"3. Получение всех проектов для пользователя {GithubRequest.Owner}");
				Console.WriteLine($"4. Получение список задач с канбана");
				Console.WriteLine($"5. Перемещение задачи с одного столбца в другой");

				var input = Console.ReadLine();
				if (input.ToLower() == "end")
				{
					break;
				}

				switch (int.Parse(input))
				{
					case 1:
						GithubRequest.GetListIssues();
						break;

					case 2:
						GithubRequest.CreateIssue();
						break;

					case 3:
						GithubRequest.GetProjectsForUser();
						break;

					case 4:
						GithubRequest.GetTasksFromCanban();
						break;

					case 5:
						GithubRequest.MoveIssues();
						break;
				}
			}

			Console.ReadKey();
		}
	}
}

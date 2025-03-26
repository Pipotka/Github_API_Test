using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Github_API_Test.Models.Requests;
using Github_API_Test.Models.Responces;

namespace Github_API_Test
{
	internal static class GithubRequest
	{
		public static string Owner = "Pipotka";
		public static string Repos = "Github_API_Test";

		public static void CreateIssue()
		{
			var url = $"https://api.github.com/repos/{Owner}/{Repos}/issues";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "POST";

			var createdIssue = new CreateIssueRequest();
			Console.Write("Название проблемы: ");
			createdIssue.Title = Console.ReadLine();
			Console.Write("Необязательно. Описание проблемы: ");
			createdIssue.Body = Console.ReadLine();

			var jsonBody = JsonConvert.SerializeObject(createdIssue, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});

			SetBodyInRequest(req, jsonBody);

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");

			Console.WriteLine();
			Console.WriteLine($"Тело запроса: {jsonBody}");

			var res = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(res.GetResponseStream());

			var response = reader.ReadToEnd();
			PrintResponce(response);
		}

		public static void GetListIssues()
		{
			var url = $"https://api.github.com/repos/{Owner}/{Repos}/issues";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "GET";

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");

			var res = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(res.GetResponseStream());
			var response = reader.ReadToEnd();
			PrintResponce(response);

			var issues = JsonConvert.DeserializeObject<IssueResponse[]>(response);

			Console.WriteLine();
			var index = 1;
			foreach (IssueResponse issue in issues)
			{
				Console.WriteLine();
				Console.WriteLine($"Issue{index} : {{ \n Id: {issue.Id}; \n Название: {issue.Title}; \n Описание: {issue.Body}; \n }}");
				index++;
			}
		}

		public static UserProjectResponse[] GetProjectsForUser()
		{
			var url = "https://api.github.com/graphql";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "POST";

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");

			var body = new UserProjectsRequest(Owner, 5);

			var jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});

			SetBodyInRequest(req, jsonBody);

			Console.WriteLine();
			Console.WriteLine($"Тело запроса : {jsonBody}");

			var res = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(res.GetResponseStream());
			var response = reader.ReadToEnd();
			PrintResponce(response);

			var jObject = JObject.Parse(response);
			var nodes = jObject["data"]["user"]["projectsV2"]["nodes"].ToString();

			var projects = JsonConvert.DeserializeObject<UserProjectResponse[]>(nodes);

			Console.WriteLine();
			
			foreach (var project in projects)
			{
				Console.WriteLine();
				Console.WriteLine($"Проект : {{ \n Id: {project.Id}; \n Название: {project.Title}; \n }}");
			}

			return projects;
		}

		public static void MoveIssues()
		{
			var projectAndCanbanAndIssues = GetTasksFromCanban();
			var issues = projectAndCanbanAndIssues.issues;
			var project = projectAndCanbanAndIssues.project;
			var canban = projectAndCanbanAndIssues.canban;
			var groupedIssuesByColumn = issues.GroupBy(x => x.FieldValues.Nodes.First().Name);

			Console.WriteLine();
			foreach (var column in groupedIssuesByColumn)
			{
				Console.WriteLine($"Столбец {column.Key} {{");
				foreach (var issue in column)
				{
					Console.WriteLine($"\tId:{issue.Content.Id}; \n\tНазвание:{issue.Content.Title}; \n\tОписание:{issue.Content.Body}; \n");
				}
				Console.WriteLine($"}} \n");
			}

			Console.Write("Выберите проблему. Введите id выбранной проблемы: ");
			var id = Console.ReadLine();
			var selectedIssue = issues.First(x => x.Content.Id == id);
			Console.WriteLine($"Вы выбрали проблему с названием \"{selectedIssue.Content.Title}\"");

			var columns = groupedIssuesByColumn.Select((x, index) => (index, x.Key)).ToDictionary(x => x.index, x => x.Key);
			Console.WriteLine();
			foreach (var column in columns)
			{
				Console.WriteLine($"{column.Key}. {column.Value}");
			}
			Console.Write($"Выберите столбец, в который хотите переместить проблему {selectedIssue.Content.Title}: ");
			var columnIndex = int.Parse(Console.ReadLine());
			var selectedColumn = canban.Options.First(x => x.Name == columns[columnIndex]);

			var url = "https://api.github.com/graphql";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "POST";

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");


			var jsonBody = GraphQLQuerys.GetQueryForMoveIssues(project.Id, selectedIssue.Id, canban.Id, selectedColumn.Id);

			SetBodyInRequest(req, jsonBody);

			Console.WriteLine();
			Console.WriteLine($"Тело запроса : {jsonBody}");

			try
			{
				var res = (HttpWebResponse)req.GetResponse();
				var reader = new StreamReader(res.GetResponseStream());
				var response = reader.ReadToEnd();
				PrintResponce(response);
				Console.WriteLine("Перемещение выполнено!");
			}
			catch (WebException ex) when (ex.Response != null)
			{
				using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
				using (Stream errorStream = errorResponse.GetResponseStream())
				using (StreamReader erorReader = new StreamReader(errorStream))
				{
					string errorText = erorReader.ReadToEnd();
					Console.WriteLine($"Ошибка {(int)errorResponse.StatusCode} ({errorResponse.StatusCode}):");
					Console.WriteLine(errorText);
				}
			}
		}

		public static void PrintResponce(string responce)
		{
			var parsedResponse = JToken.Parse(responce);
			Console.WriteLine();
			Console.WriteLine($"Ответ : \n{parsedResponse.ToString(Formatting.Indented)}");
		}

		public static (ICollection<CanbanIssiesResponce> issues, UserProjectResponse project, SingleSelectFieldResponce canban) GetTasksFromCanban()
		{
			var projects = GetProjectsForUser();
			var canban = GetCanban(projects[0]);
			var columnsDictionry = canban.Options
				.Select((x, index) => (index, x.Id, x.Name))
				.ToDictionary(x => x.index, x => (x.Id, x.Name));

			Console.WriteLine();
			foreach (var column in columnsDictionry)
			{
				Console.WriteLine($"{column.Key}: {column.Value.Name}");
			}
			Console.Write("Выберите столбец: ");
			var input = int.Parse(Console.ReadLine());
			var selected = columnsDictionry[input];


			var url = "https://api.github.com/graphql";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "POST";

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");

			var body = new GraphQLRequest();
			body.Query = GraphQLQuerys.GetQueryForIssuesFromColumn(projects[0].Id);

			var jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});

			SetBodyInRequest(req, jsonBody);

			Console.WriteLine();
			Console.WriteLine($"Тело запроса : {jsonBody}");

			var res = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(res.GetResponseStream());
			var response = reader.ReadToEnd();
			PrintResponce(response);

			var jObject = JObject.Parse(response);
			var nodes = jObject["data"]["node"]["items"]["nodes"].ToString();
			var issues = JsonConvert.DeserializeObject<CanbanIssiesResponce[]>(nodes)
				.Where(x => x.Id != null);
			foreach (var issue in issues)
			{
				issue.FieldValues.Nodes = issue.FieldValues.Nodes.Where(x => x.Name != null).ToArray();
			}
			var backlogIssues = issues
				.Where(x => x.FieldValues.Nodes.First().Name == selected.Name);

			Console.WriteLine();
			Console.WriteLine($"Проблемы из {selected.Name}: {{");
			foreach (var issue in backlogIssues)
			{
				Console.WriteLine($"\tId: {issue.Content.Id}; \n\tНазвание: {issue.Content.Title};\n\tОписание: {issue.Content.Body};\n");
			}
			Console.WriteLine($"}}");
			return (issues.ToArray(), projects[0], canban);
		}

		public static SingleSelectFieldResponce GetCanban(UserProjectResponse project)
		{
			var url = "https://api.github.com/graphql";
			var req = (HttpWebRequest)WebRequest.Create(url);
			FillHeaders(req);
			req.Method = "POST";

			Console.WriteLine();
			Console.WriteLine($"Запрос отправлен по url: {url}");

			var body = new SingleSelectFieldRequest(project.Id, 5);

			var jsonBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});

			SetBodyInRequest(req, jsonBody);

			Console.WriteLine();
			Console.WriteLine($"Тело запроса : {jsonBody}");

			var res = (HttpWebResponse)req.GetResponse();
			var reader = new StreamReader(res.GetResponseStream());
			var response = reader.ReadToEnd();
			PrintResponce(response);

			var jObject = JObject.Parse(response);
			var nodes = jObject["data"]["node"]["fields"]["nodes"].ToString();

			var resultProject = JsonConvert.DeserializeObject<SingleSelectFieldResponce[]>(nodes)
				.First(x => x.Id != null);
			resultProject.Options = resultProject.Options.Where(x => x.Id != null).ToArray();

			Console.WriteLine();

			Console.WriteLine();
			Console.WriteLine($"Узел : {{ \n Id: {resultProject.Id}; \n Название: {resultProject.Name};");
			foreach (var innerNode in resultProject.Options)
			{
				Console.WriteLine($"\tВнутренний узел: {{ \n \t\tId: {innerNode.Id}; \n \t\tНазвание: {innerNode.Name};");
			}

			return resultProject;
		}

		static void SetBodyInRequest(HttpWebRequest req, string body)
		{
			using (var streamWriter = new StreamWriter(req.GetRequestStream()))
			{
				streamWriter.Write(body);
				streamWriter.Flush();
				streamWriter.Close();
			}
		}

		static void FillHeaders(HttpWebRequest req)
		{
			req.Headers.Add(HttpRequestHeader.Authorization, "Bearer Ваш персональный токен доступа");
			req.UserAgent = "StopapupaApp";
			req.Accept = "application/json";
			req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
		}
	}
}

using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Quiz;

[Authorize]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cacheService;

	public IndexModel(ApplicationDbContext db, ICacheService cacheService)
	{
		_db = db;
		_cacheService = cacheService;
	}

	public string Error { get; set; } = "";
	public string QuizState { get; set; } = "";

	public int Q2Answer { get; set; } = 0;
	public int Q2Min { get; set; } = 0;
	public int Q2Max { get; set; } = 0;
	public int Q3Answer { get; set; } = 0;
	public int Q3Min { get; set; } = 0;
	public int Q3Max { get; set; } = 0;
	public int Q4Answer { get; set; } = 0;
	public int Q4Max { get; set; } = 0;

	public int Q1Below { get; set; } = 0;
	public int Q1Correct { get; set; } = 0;
	public int Q1Above { get; set; } = 0;

	public int Q2Below { get; set; } = 0;
	public int Q2Close { get; set; } = 0;
	public int Q2Correct { get; set; } = 0;
	public int Q2Above { get; set; } = 0;

	public int Q3Below { get; set; } = 0;
	public int Q3Close { get; set; } = 0;
	public int Q3Correct { get; set; } = 0;
	public int Q3Above { get; set; } = 0;

	public int Q4Below { get; set; } = 0;
	public int Q4Correct { get; set; } = 0;
	public int Q4Above { get; set; } = 0;

	public int Q5Speedrun { get; set; } = 0;
	public int Q5Superplay { get; set; } = 0;

	public int Q6Correct { get; set; } = 0;
	public int Q6Wrong { get; set; } = 0;

	public int Q7Below { get; set; } = 0;
	public int Q7Close { get; set; } = 0;
	public int Q7Correct { get; set; } = 0;
	public int Q7Above { get; set; } = 0;

	public int Q8GIF { get; set; } = 0;
	public int Q8JIF { get; set; } = 0;

	public int Q9Lever { get; set; } = 0;
	public int Q9Nothing { get; set; } = 0;

	public int Q10Consent { get; set; } = 0;
	public int Q10Agree { get; set; } = 0;
	public int Q10Reject { get; set; } = 0;

	public int Q11Wait { get; set; } = 0;
	public int Q11NoWait { get; set; } = 0;

	public async Task<IActionResult> OnGet()
	{
		try
		{
			Q2Answer = await _db.Publications.CountAsync();
			Q3Answer = await _db.Submissions.Where(s => s.Status == SubmissionStatus.Rejected || s.Status == SubmissionStatus.Cancelled).CountAsync();
			Q4Answer = await _db.Submissions.Where(s => s.CreateTimestamp >= new DateTime(2023, 4, 1, 0, 0, 0, DateTimeKind.Utc)).CountAsync();

			Q2Min = Q2Answer - (Q2Answer % 100) - 700;
			Q2Max = Q2Min + 1000;
			Q3Min = Q3Answer - (Q3Answer % 100) - 200;
			Q3Max = Q3Min + 1000;
			Q4Max = Math.Max(9, (int)(Q4Answer * 1.5));

			_cacheService.TryGetValue("april-2023-data", out Dictionary<int, Dictionary<int, string>>? data);
			if (data != null)
			{
				QuizState = JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true });
				if (data.TryGetValue(1, out Dictionary<int, string>? q1data))
				{
					foreach (string v in q1data.Values.ToList())
					{
						if (int.TryParse(v, out int frms))
						{
							if (frms < 13)
							{
								Q1Below++;
							}
							else if (frms == 13)
							{
								Q1Correct++;
							}
							else if (frms > 13)
							{
								Q1Above++;
							}
						}
					}
				}

				if (data.TryGetValue(2, out Dictionary<int, string>? q2data))
				{
					foreach (string v in q2data.Values.ToList())
					{
						var value = JsonSerializer.Deserialize<Tuple<int, int>>(v);
						if (value is not null)
						{
							if (value.Item1 < value.Item2 - 100)
							{
								Q2Below++;
							}
							else if (value.Item1 == value.Item2)
							{
								Q2Correct++;
								Q2Close++;
							}
							else if (value.Item1 >= value.Item2 - 100 && value.Item1 <= value.Item2 + 100)
							{
								Q2Close++;
							}
							else if (value.Item1 > value.Item2 + 100)
							{
								Q2Above++;
							}
						}
					}
				}

				if (data.TryGetValue(3, out Dictionary<int, string>? q3data))
				{
					foreach (string v in q3data.Values.ToList())
					{
						var value = JsonSerializer.Deserialize<Tuple<int, int>>(v);
						if (value is not null)
						{
							if (value.Item1 < value.Item2 - 100)
							{
								Q3Below++;
							}
							else if (value.Item1 == value.Item2)
							{
								Q3Correct++;
								Q3Close++;
							}
							else if (value.Item1 >= value.Item2 - 100 && value.Item1 <= value.Item2 + 100)
							{
								Q3Close++;
							}
							else if (value.Item1 > value.Item2 + 100)
							{
								Q3Above++;
							}
						}
					}
				}

				if (data.TryGetValue(4, out Dictionary<int, string>? q4data))
				{
					foreach (string v in q4data.Values.ToList())
					{
						var value = JsonSerializer.Deserialize<Tuple<int, int>>(v);
						if (value is not null)
						{
							if (value.Item1 < value.Item2)
							{
								Q4Below++;
							}
							else if (value.Item1 == value.Item2)
							{
								Q4Correct++;
							}
							else if (value.Item1 > value.Item2)
							{
								Q4Above++;
							}
						}
					}
				}

				if (data.TryGetValue(5, out Dictionary<int, string>? q5data))
				{
					foreach (string v in q5data.Values.ToList())
					{
						if (v == "Speedrun")
						{
							Q5Speedrun++;
						}
						else if (v == "Superplay")
						{
							Q5Superplay++;
						}
					}
				}

				if (data.TryGetValue(6, out Dictionary<int, string>? q6data))
				{
					foreach (string v in q6data.Values.ToList())
					{
						if (v == "6")
						{
							Q6Correct++;
						}
						else
						{
							Q6Wrong++;
						}
					}
				}

				if (data.TryGetValue(7, out Dictionary<int, string>? q7data))
				{
					foreach (string v in q7data.Values.ToList())
					{
						var value = JsonSerializer.Deserialize<Tuple<string, string>>(v);
						if (value is not null && double.TryParse(value.Item1, NumberStyles.Float, CultureInfo.InvariantCulture, out double item1) && double.TryParse(value.Item2, NumberStyles.Float, CultureInfo.InvariantCulture, out double item2))
						{
							if (item1 < item2 - 10)
							{
								Q7Below++;
							}
							else if (item1 == item2)
							{
								Q7Correct++;
								Q7Close++;
							}
							else if (item1 >= item2 - 10 && item1 <= item2 + 10)
							{
								Q7Close++;
							}
							else if (item1 > item2 + 10)
							{
								Q7Above++;
							}
						}
					}
				}

				if (data.TryGetValue(8, out Dictionary<int, string>? q8data))
				{
					foreach (string v in q8data.Values.ToList())
					{
						if (v == "GIF")
						{
							Q8GIF++;
						}
						else if (v == "JIF")
						{
							Q8JIF++;
						}
					}
				}

				if (data.TryGetValue(9, out Dictionary<int, string>? q9data))
				{
					foreach (string v in q9data.Values.ToList())
					{
						if (v == "Lever")
						{
							Q9Lever++;
						}
						else if (v == "Nothing")
						{
							Q9Nothing++;
						}
					}
				}

				if (data.TryGetValue(10, out Dictionary<int, string>? q10data))
				{
					foreach (string v in q10data.Values.ToList())
					{
						if (v == "Consent")
						{
							Q10Consent++;
						}
						else if (v == "Agree")
						{
							Q10Agree++;
						}
						else if (v == "Reject")
						{
							Q10Reject++;
						}
					}
				}

				if (data.TryGetValue(11, out Dictionary<int, string>? q11data))
				{
					foreach (string v in q11data.Values.ToList())
					{
						if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
						{
							if (value < 20)
							{
								Q11NoWait++;
							}
							else if (value >= 20)
							{
								Q11Wait++;
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Error = ex.Message;
		}

		return Page();
	}

	public async Task<IActionResult> OnPostAnswers()
	{
		try
		{
			var answers = await JsonSerializer.DeserializeAsync<Dictionary<int, JsonElement>>(Request.Body);
			var validDict = new Dictionary<int, string>();
			if (answers is not null)
			{
				for (int i = 1; i <= 11; i++)
				{
					if (answers.TryGetValue(i, out JsonElement v))
					{
						if (i == 1 && v.ValueKind == JsonValueKind.String)
						{
							string? val = v.GetString();
							if (int.TryParse(val, out int value))
							{
								if (value >= -1 && value <= 69)
								{
									validDict[i] = value.ToString();
								}
							}
						}
						else if ((i == 2 || i == 3 || i == 4) && v.ValueKind == JsonValueKind.Array)
						{
							List<string?> val = v.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.Number ? e.GetDouble().ToString() : e.GetString()).ToList();
							if (int.TryParse(val[0], out int item1) && int.TryParse(val[1], out int item2))
							{
								validDict[i] = JsonSerializer.Serialize(new Tuple<int, int>(item1, item2));
							}
						}
						else if ((i == 5 || i == 6 || i == 8 || i == 9 || i == 10 || i == 11) && v.ValueKind == JsonValueKind.String)
						{
							string? val = v.GetString();
							if (val != null)
							{
								validDict[i] = val;
							}
						}
						else if (i == 7 && v.ValueKind == JsonValueKind.Array)
						{
							List<string?> val = v.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.Number ? e.GetDouble().ToString() : e.GetString()).ToList();
							if (double.TryParse(val[0], out double item1) && double.TryParse(val[1], out double item2))
							{
								validDict[i] = JsonSerializer.Serialize(new Tuple<string, string>(item1.ToString(CultureInfo.InvariantCulture), item2.ToString(CultureInfo.InvariantCulture)));
							}
						}
					}
				}
			}

			int userId = User.GetUserId();
			_cacheService.TryGetValue("april-2023-data", out Dictionary<int, Dictionary<int, string>>? data);
			if (data == null)
			{
				data = new Dictionary<int, Dictionary<int, string>>();
				for (int i = 1; i <= 11; i++)
				{
					data[i] = new Dictionary<int, string>();
				}
			}

			for (int i = 1; i <= 11; i++)
			{
				if (validDict.ContainsKey(i) && !data[i].ContainsKey(userId))
				{
					data[i][userId] = validDict[i];
				}
			}

			_cacheService.Set("april-2023-data", data, Durations.OneWeekInSeconds);
		}
		catch
		{
		}

		return new ContentResult { StatusCode = StatusCodes.Status200OK };
	}

	public async Task<IActionResult> OnPostCache()
	{
		if (User.Name() == "Masterjun")
		{
			Dictionary<int, Dictionary<int, string>>? data;
			try
			{
				data = await JsonSerializer.DeserializeAsync<Dictionary<int, Dictionary<int, string>>>(Request.Body);
			}
			catch (Exception ex)
			{
				return new ContentResult { StatusCode = StatusCodes.Status400BadRequest, Content = ex.Message };
			}

			_cacheService.Set("april-2023-data", data, Durations.OneWeekInSeconds);
		}

		return new ContentResult { StatusCode = StatusCodes.Status200OK };
	}

	public async Task<IActionResult> OnPostCacheText()
	{
		if (User.Name() == "Masterjun")
		{
			string text;
			try
			{
				using StreamReader sr = new(Request.Body);
				text = await sr.ReadToEndAsync();
			}
			catch (Exception ex)
			{
				return new ContentResult { StatusCode = StatusCodes.Status400BadRequest, Content = ex.Message };
			}

			_cacheService.Set("april-2023-text", text, Durations.OneWeekInSeconds);
		}

		return new ContentResult { StatusCode = StatusCodes.Status200OK };
	}
}

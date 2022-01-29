﻿using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Models;

public class AtLeastOneAttribute : ValidationAttribute
{
	public AtLeastOneAttribute()
	{
		ErrorMessage = "At least one selection is required.";
	}

	public override bool IsValid(object? value)
	{
		if (value is IEnumerable list)
		{
			return list.Cast<object?>().Any();
		}

		return false;
	}
}

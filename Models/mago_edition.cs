using System.Collections.Generic;

namespace ADLoginAPI.Models
{
	public class mago_edition
	{
		public int id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string? code_type { get; set; }
		public string? suffix { get; set; }
		public int version_id { get; set; }

		public virtual mago_version version { get; set; }
	}
}
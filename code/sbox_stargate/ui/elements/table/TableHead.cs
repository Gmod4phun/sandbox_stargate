using System.Collections.Generic;
using Sandbox;
using Sandbox.Html;
using Sandbox.UI;

public partial class TableHead : Panel {

	public List<Panel> Columns = new();

	public TableHead() {
		AddClass("table-head");
	}

	public void AddColumn(string name) {
		var col = new Panel();
		col.AddClass("head-column");
		var label = col.AddChild<Label>();
		label.Text = name;

		Columns.Add(col);
		AddChild(col);
	}

	public override bool OnTemplateElement( INode element )
	{
		var columnsNames = element.InnerHtml.Split(',');
		foreach (string col in columnsNames)
			AddColumn(col);
		return true;
	}

}

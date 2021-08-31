using Sandbox.Html;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.UI
{
	public class Option
	{
		public string Title;
		public string Icon;
		public string Subtitle;
		public string Tooltip;
		public object Value;
	}

	[Library( "select" )]
	public class DropDown : PopupButton
	{
		protected IconPanel DropdownIndicator;

		public List<Option> Options { get; } = new();

		public DropDown()
		{
			StyleSheet.Load("/ui/elements/dropdown/dropdown.scss");
			AddClass( "dropdown" );
			DropdownIndicator = Add.Icon( "expand_more", "dropdown_indicator" );
		}

		public override void SetProperty( string name, string value )
		{
			base.SetProperty( name, value );

			if ( name == "value" )
			{
				Select( value, false );
			}
		}

		public override void Open()
		{
			Popup = new Popup( this, Popup.PositionMode.BelowStretch, 4.0f );
			Popup.AddClass( "flat-top" );

			foreach( var option in Options )
			{
				var o = Popup.AddOption( option.Title, option.Icon, () => Select( option ) );
				if ( Selected != null && option.Value == Selected.Value )
				{
					o.AddClass( "active" );
				}
			}
		}

		protected virtual void Select( Option option, bool triggerChange = true )
		{
			if ( !triggerChange )
			{
				selected = option;

				if ( option != null )
				{
					Value = $"{option.Value}";
					Icon = option.Icon;
					Text = option.Title;
				}
			}
			else
			{
				Selected = option;
			}
		}		
		
		protected virtual void Select( string value, bool triggerChange = true )
		{
			if ( Value == value ) return;
			Value = value;

			Select( Options.FirstOrDefault( x => string.Equals( x.Value.ToString(), value, StringComparison.OrdinalIgnoreCase ) ), triggerChange );
		}

		public DropDown AddOption( string title, string value ) {
			Option o = new();
			o.Title = title;
			o.Value = value;
			Options.Add(o);

			return this;
		}

		public string Value { get; protected set; }

		Option selected;

		public Option Selected 
		{
			get => selected;
			set
			{
				if ( selected == value ) return;

				selected = value;

				if ( selected != null )
				{
					Value = $"{selected.Value}";
					Icon = selected.Icon;
					Text = selected.Title;

					CreateValueEvent( "value", selected?.Value );
				}
			}
		}

		public override bool OnTemplateElement( INode element )
		{
			Options.Clear();

			foreach ( var child in element.Children )
			{
				if ( !child.IsElement ) continue;

				if ( child.Name.Equals( "option", StringComparison.OrdinalIgnoreCase ) )
				{
					Option o = new();

					o.Title = child.InnerHtml;
					o.Value = child.GetAttribute( "value", o.Title );
					o.Icon = child.GetAttribute( "icon", null );

					Options.Add( o );

					if (child.GetAttribute( "selected" ) == "true") {
						Select(o, false);
					}
				}
			}

			// Select( Value );
			return true;
		}

		public override void OnMouseWheel(float value) {
			// 1 = Down
			// -1 = Up

			var index = Options.FindIndex(x => x == selected);
			if (index == -1) {
				Select(value == 1 ? Options.First() : Options.Last());
				return;
			}

			if (value == 1) {
				if (index == Options.Count - 1)
					return;

				Select(Options[index + 1]);
			} else {
				if (index == 0)
					return;

				Select(Options[index - 1]);
			}
		}
	}
}

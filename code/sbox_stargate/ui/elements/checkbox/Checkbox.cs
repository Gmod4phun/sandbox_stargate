using Sandbox.Html;
using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace Sandbox.UI
{
	[Library( "checkbox" )]
	public class Checkbox : Panel
	{
		/// <summary>
		/// The checkmark icon. Although no guarentees it's an icon!
		/// </summary>
		public Panel CheckMark { get; protected set; }

		protected bool isChecked = false;

		protected bool isDisabled = false;

		protected Panel _tooltip;
		protected string tooltip;

		/// <summary>
		/// Returns true if this checkbox is checked
		/// </summary>
		public bool Checked 
		{ 
			get => isChecked;
			set
			{
				if ( isChecked == value ) 
					return;

				isChecked = value;
				OnValueChanged();
			}
		}

		public bool Disabled
		{
			get => isDisabled;
			set
			{
				if ( isDisabled == value )
					return;

				isDisabled = value;

				if (isDisabled)
					AddClass("disabled");
				else
					RemoveClass("disabled");
			}
		}

		public string Tooltip {
			get => tooltip;
			set {
				tooltip = value;

				if (_tooltip != null)
					_tooltip.Delete(true);

				_tooltip = Add.Panel("tooltip");
				// _tooltip.Style.Position = PositionMode.Absolute;
				// _tooltip.Style.Top = (Length)(Style.Top.Value.Value + 5);
				// _tooltip.Style.Left = Style.Left;
				// _tooltip.Style.Dirty();
				_tooltip.AddChild<Label>().Text = value;
			}
		}

		public Label Label { get; protected set; }

		public string LabelText
		{
			get => Label?.Text;
			set
			{
				if ( Label == null )
				{
					Label = Add.Label();
				}

				Label.Text = value;
			}
		}

		public Checkbox()
		{
			StyleSheet.Load( "sbox_stargate/ui/elements/checkbox/checkbox.scss" );
			AddClass( "checkbox" );
			CheckMark = Add.Icon( "check", "checkmark" );
		}

		public override void SetProperty( string name, string value )
		{
			base.SetProperty( name, value );

			if ( name == "checked" || name == "value" )
			{
				Checked = value.ToBool();
			}
		}

		public override void SetContent( string value )
		{
			LabelText = value;
		}

		public virtual void OnValueChanged()
		{
			UpdateState();
			CreateEvent( "onchange", Checked );

			if ( Checked )
			{
				CreateEvent( "onchecked" );
			}
			else
			{
				CreateEvent( "onunchecked" );
			}
		}

		protected virtual void UpdateState()
		{
			SetClass( "checked", Checked );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if (!Disabled) {
				Checked = !Checked;
				CreateValueEvent( "checked", Checked );
				CreateValueEvent( "value", Checked );
			}
			e.StopPropagation();
		}

		public override bool OnTemplateElement(INode element) {
			base.OnTemplateElement(element);

			var tlp = element.GetAttribute("tooltip", null);
			if (tlp != null) {
				Tooltip = tlp;
			}

			return true;
		}
	}
}

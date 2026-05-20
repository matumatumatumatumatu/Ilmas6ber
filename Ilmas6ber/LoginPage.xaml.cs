namespace Ilmas6ber;

public partial class LoginPage : ContentPage
{
	TableView tabelview;
	public LoginPage()
	{

		new TableSection("Logi sisse")
		{
			new EntryCell
			{
				Label = "Email",
				Placeholder = "john.doe@email.com",
				Keyboard = Keyboard.Email,
			},
			new EntryCell
			{
				Label = "Password",
				Placeholder = "vähemalt 6 tähte",
				Keyboard = Keyboard.Default
			},
		};
		Content = tabelview;
	}
}
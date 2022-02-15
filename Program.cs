// See https://aka.ms/new-console-template for more information
using CsvHelper;
using System.Globalization;
using YahooFinanceApi;
using QuickMailer;
using System.Net.Mail;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;

Console.WriteLine("START!");



Settings settings;
using (StreamReader r = new StreamReader("settings.json"))
{
    string json = r.ReadToEnd();
    settings = JsonConvert.DeserializeObject<Settings>(json);
}



using (var reader = new StreamReader("stocks.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var list = csv.GetRecords<Stocks>();

    var securities = await Yahoo.Symbols(list.Select(x => x.Stock).ToArray()).Fields(Field.Symbol, Field.RegularMarketPrice, Field.FiftyTwoWeekHigh).QueryAsync();
    foreach (var item in list)
    {
        var aapl = securities[item.Stock];
        var price = aapl[Field.RegularMarketPrice];
        item.ActualValue = price;

        if (price <= item.MinValue && price >= item.MaxValue)
            item.Alertar = true;
    }




    MailMessage mailMessage = new MailMessage();
    mailMessage.Subject = "[STOCK ALERTS]";
    mailMessage.Body = "ALERTA DE AGORA -  " + DateTime.Now + " <br><br><br>";

    mailMessage.Body += "STOCK   ACTUAL     MIN      MAX   <br>";
    foreach (var item in list)
    {
        mailMessage.Body += item.Stock + " " + item.ActualValue + " " + item.MinValue+ " " + item.MaxValue + " <br>";
    }

    List<string> toMailAddress = new();
    Email email = new();
    toMailAddress.AddRange(settings.EmailsTO);

    bool isSend = email.SendEmail(toMailAddress, settings.Email, settings.Password, mailMessage.Subject, mailMessage.Body);

}










public class Settings
{
    public string[] EmailsTO { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }



}

public class Stocks
{
    [Index(0)]
    public string Stock { get; set; }
    [Index(1)]
    public decimal MinValue  { get; set; }
    [Index(2)]
    public decimal MaxValue { get; set; }

    [Ignore]
    public decimal ActualValue { get; set; }

    [Ignore]
    public bool Alertar { get; set; }
}
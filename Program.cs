using Microsoft.Extensions.Configuration;
using RestSharp;

// 1. Charger la configuration
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

// --- CONFIGURATION (Les Identifiants) ---
var urlBase = "https://api.intacct.com/ia/api/v1/";
var idClient = config["IdClient"];
var secretClient = config["SecretClient"];
var utilisateur = config["Utilisateur"];

var intacctService = new IntacctService(urlBase, idClient, secretClient, utilisateur);
var reponseAuth = await intacctService.ObtenirToken();

if (!reponseAuth.IsSuccessful)
{
    Console.WriteLine("Erreur lors de l'authentification : " + reponseAuth.Content);
    return;
}

var intacctToken = new IntacctToken(reponseAuth);
Console.WriteLine("Token d'accès : " + intacctToken.AccessToken.Substring(0, 40));
Console.WriteLine("Date d'expiration : " + intacctToken.DateExpiration);
Console.WriteLine("Est expiré ? : " + intacctToken.EstExpire);
Console.ReadLine();

// Rafraichir un token
var reponseRafraichir = await intacctService.RafraichirToken(intacctToken.RefreshToken);


// Revoker un token
//var revokerToken = await intacctService.RevokerToken(reponseAuth.AccessToken);
Console.ReadLine();
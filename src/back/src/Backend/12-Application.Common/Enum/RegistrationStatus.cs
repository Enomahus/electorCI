namespace Application.Common.Enums;

public enum RegistrationStatus
{
    Draft, // Demande enrégistrée en tant que brouillon attendant de soumettre
    ToBeProcessed, // Demande enregistré en attente de traitement au niveau de la commission
    Approved, // Agent à traiter et valider la demande => Electeur créé avec son numéro
    Rejected, // Demande rejetée pour raison donnée (Dossier incomplet, defaut de certificat de nationalité, defaut de CNI)
}

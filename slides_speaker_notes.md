# Notes orateur (addendum Batch)

À ce stade, on a vu le CRUD classique en appel unitaire. Maintenant on ajoute le Batch, qui permet de traiter plusieurs enregistrements d'un même objet dans une seule requête.

Le Batch n'est pas du Bulk : Bulk est asynchrone avec jobId et fichier, alors que Batch répond directement avec un statut par enregistrement.

Pour un GET batch, on met plusieurs clés dans le path, séparées par des virgules. Pour un POST batch, on envoie un tableau JSON d'objets à créer. Pour un PATCH batch, on envoie un tableau d'objets avec la key dans chaque élément. Pour un DELETE batch, on passe les clés dans le path, comme le GET.

Il faut retenir la limite de 500 enregistrements maximum par requête. Si on veut un traitement transactionnel tout-ou-rien, on ajoute le header X-IA-API-Param-Transaction: true pour le mode atomic.

Dans la réponse, on lit ia::result pour le statut ligne par ligne, puis ia::meta pour le résumé global totalSuccess et totalError.

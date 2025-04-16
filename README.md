# SageWebAPI

## Description

SageWebApi est une application qui utilise un moniteur de service broker pour surveiller les changements dans une base de données SQL Sage 100c. Le moniteur est implémenté dans la classe 
ServiceBrokerMonitor située dans le fichier SageWebApi/Models/ServiceBrokerMonitor.cs. Le moniteur utilise le service broker de SQL Server pour recevoir des notifications de changement de données. 

## Fonctionnalités

### Surveillance des changements
    Le moniteur surveille les changements dans les tables Document, Tiers, Ecriture et Echéance de la base de données.

### Gestion des événements
    La classe ServiceBrokerMonitor déclenche des événements lorsque des changements sont détectés dans les tables surveillées.

### Gestion des connexions
    La classe gère les connexions à la base de données et assure une fermeture propre des connexions lors de l'arrêt du moniteur.

### Annulation des tâche
    Le moniteur peut annuler les tâches de surveillance en cours pour permettre un arrêt propre.


## Contribution
Les contributions sont les bienvenues ! Veuillez ouvrir une issue ou soumettre une pull request pour toute amélioration ou correction.

## Licence
Ce projet est sous licence MIT. Voir le fichier LICENSE pour plus de détails.
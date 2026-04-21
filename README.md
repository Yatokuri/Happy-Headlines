# Happy-Headlines
Compulsory Assignment #1 - Happy Headlines - Development of Large Systems



## Setup
1) Go to root
2) Run docker compose up --build
3) Open Featurehub - http://localhost:8085
4) Setup account + serviceAccount for FeatureHub
5) Add FeatureFlag - "SubscriberServiceEnabled"
6) Create .env based upon .env.example in repo root next to compose.yaml
6) Fetch Api key from ServiceAccount & insert into the .env file from above in the "FEATUREHUB_API_KEY"
7) Run docker compose up -d --force-recreate subscriberservice

## Swagger endpoint ports

### Articleservice
http://localhost:8080/swagger
### ProfanityService
http://localhost:5002/swagger
### CommentService
http://localhost:5003/swagger
### DraftService
http://localhost:5004/swagger
### NewsletterService
http://localhost:5005/swagger
### PublisherService
http://localhost:5006/swagger
### SubscriberService
http://localhost:5007/swagger

## Seq, Zipkin, Grafana
### Seq
http://localhost:5341
### Zipkin
http://localhost:9411
### Grafana
http://localhost:3000  
**Username:** admin  
**Password:** admin

## FeatureHub
### FeatureHub
http://localhost:8085

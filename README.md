# CardGame

This is a educational project to implement a simple card game. There will be a central server to which multiple clients will connect.

## Requirements

- You will need to install NodeJs version 12.16.1 or greater
  - https://nodejs.org/en/download/

- Update npm to version 6.13.7 or greater
  - npm install -g npm

- Angular 9 or higher
  - This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 9.0.3.

## To build

- Compile the client
  - ng build --prod --delete-output-path --output-path=Out
- Copy client output to server host directory
  - cp -r Out/* ../CardGame.Server/ClientApp/dist

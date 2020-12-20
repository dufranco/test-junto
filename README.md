# test-junto
 Teste técnico para Junto Seguros feito por Eduardo Franco de Lima

# Orientações para rodar a aplicação
- IDE: Visual Studio Community;
- Banco de dados: SQL Server;
- Versão migrations: 3.0.0;
- Roda no IIS Express.

## Rodando a aplicação
- Atentar-se a string de conexão com o banco de dados;
- Rodar a migration para criar o banco de dados; (dotnet ef database update);
- Toda requisição relacionada a login retorna o token e a data de expiração do mesmo, exceto os endpoints /test-authentication e /info;
- É preciso cadastrar um usuário para que seja possível realizar o login;
- A senha deve atender os critérios mínimos;
- O reset de senha precisa de token para ser efetivado;
- Bloqueios: cadastro duplicado e requisição sem token;
- Testes feitos em NUnit;
- Testes de requisições feitos pelo Swagger.
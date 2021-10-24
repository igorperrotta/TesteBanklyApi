# TesteBanklyApi
Solução da prova da Bankly

# Como fiz
Eu estruturei o projeto da seguinte forma:
Existe uma fila de objetos que contem todos os itens que foram e estão a ser processados. Existe um job que roda automaticamente a cada um minuto para processar os dados da fila de acordo com o status.
Caso o status seja "In queue" ele será processado e os dados serão enviados a api no heroku.
Para acessar o hangfire para ver o status dos jobs é só pegar o endereço localhost e adicionar /hangfire

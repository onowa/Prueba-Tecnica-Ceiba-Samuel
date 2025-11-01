Tecnologías Utilizadas para la Solución

Respuesta// A

Usé .NET 8 con C# porque es la tecnología que más manejo y con la que me siento cómodo trabajando. Además, me parece muy estable, sobre todo para proyectos financieros, y tiene buen soporte por parte de Microsoft. Como BTG es una empresa seria, preferí usar algo confiable que no me complicara la vida más de lo necesario.

\*Arquitectura del Proyecto

Decidí aplicar Clean Architecture porque me gusta tener el código bien ordenado y fácil de entender. Separé todo en capas: la de negocio, la de datos, etc. También usé CQRS con MediatR, ya que me ayuda a organizar mejor las operaciones, separando las consultas de los comandos. Eso hace que el código sea más claro y fácil de mantener.

\*Base de Datos - DynamoDB

Opté por DynamoDB (NoSQL) por varias razones:

Es flexible para manejar diferentes tipos de fondos.

Se escala automáticamente.

No tengo que preocuparme por servidores ni mantenimiento.

AWS se encarga de la parte pesada.

\*Validaciones y Testing

Para las validaciones usé FluentValidation, porque me permite escribir las reglas de negocio de forma sencilla y clara. Por ejemplo, validar que el correo no se repita o que el usuario tenga mayoría de edad.
También hice varias pruebas unitarias con xUnit (más de 40), porque en una app financiera es importante asegurarse de que todo funcione correctamente.

\*API y Documentación

Desarrollé una API REST y le integré Swagger para generar la documentación automáticamente. Así es más fácil probar los endpoints y que otros desarrolladores entiendan cómo funciona todo.

\*Despliegue en la Nube

Elegí AWS porque ofrece muchos servicios útiles y se puede automatizar todo con CloudFormation. Además, es una plataforma estable y no tan costosa para comenzar.
Preparé scripts que permiten desplegar toda la infraestructura con un solo comando.

\*Notificaciones

Para los correos y mensajes de texto usé SES y SNS de AWS. Son servicios confiables y así evito montar servidores de correo por mi cuenta.

Resumen

En general, traté de usar tecnologías que ya conozco y que al mismo tiempo se adapten bien a un entorno empresarial como BTG. Busqué que la solución fuera estable, escalable y fácil de mantener a futuro.

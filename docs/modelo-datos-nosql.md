*Modelo de Datos

Respuesta// B

*Tabla: Clientes

```json
{
  "NombreTabla": "Customers",
  "ClaveParticion": "Id",
  "Atributos": {
    "Id": "String (UUID)",
    "Nombre": "String",
    "Apellido": "String",
    "Email": "String (único)",
    "Telefono": "String",
    "FechaNacimiento": "String",
    "Saldo": "Number",
    "TipoDocumento": "String (CC|CE|TI)",
    "NumeroDocumento": "String",
    "Ciudad": "String",
    "PerfilRiesgo": "String (Conservador|Moderado|Agresivo)",
    "EstadoCuenta": "String (Activo|Suspendido|Cerrado)",
    "PreferenciasNotificacion": {
      "Email": "Boolean",
      "SMS": "Boolean"
    },
    "FechaCreacion": "String",
    "FechaActualizacion": "String",
    "EsActivo": "Boolean"
  },
  "IndicesSecundarios": [
    {
      "NombreIndice": "EmailIndex",
      "ClaveParticion": "Email"
    },
    {
      "NombreIndice": "DocumentoIndex",
      "ClaveParticion": "TipoDocumento",
      "ClaveOrdenamiento": "NumeroDocumento"
    }
  ]
}
```

*Tabla: Fondos

```json
{
  "NombreTabla": "Funds",
  "ClaveParticion": "Id",
  "Atributos": {
    "Id": "String (UUID)",
    "Nombre": "String",
    "Descripcion": "String",
    "MontoMinimo": "Number",
    "Categoria": "String (FPV|FIC)",
    "NivelRiesgo": "String (Bajo|Medio|Alto)",
    "Moneda": "String (COP)",
    "ComisionAdministracion": "Number",
    "PenalidadRetiroAnticipado": "Number",
    "MontoMaximo": "Number",
    "ObjetivoInversion": "String",
    "FechaCreacion": "String",
    "RendimientoAnual": "Number",
    "EsActivo": "Boolean",
    "EstadoOperacion": "String (Abierto|Cerrado|Suspendido)"
  },
  "IndicesSecundarios": [
    {
      "NombreIndice": "CategoriaIndex",
      "ClaveParticion": "Categoria"
    },
    {
      "NombreIndice": "EstadoIndex",
      "ClaveParticion": "EstadoOperacion"
    }
  ]
}
```

*Tabla: Suscripciones

```json
{
  "NombreTabla": "Subscriptions",
  "ClaveParticion": "Id",
  "Atributos": {
    "Id": "String (UUID)",
    "ClienteId": "String (UUID)",
    "FondoId": "String (UUID)",
    "MontoInicial": "Number",
    "ValorActual": "Number",
    "Unidades": "Number",
    "Estado": "String (Activa|Cancelada|Suspendida)",
    "TipoSuscripcion": "String (UnicoPago|Recurrente)",
    "MontoRecurrente": "Number",
    "FrecuenciaRecurrente": "String (Mensual|Trimestral)",
    "ProximaFechaRecurrente": "String",
    "FechaSuscripcion": "String",
    "FechaCancelacion": "String",
    "FechaVencimiento": "String",
    "PenalidadAplicada": "Number",
    "MotivoCancelacion": "String"
  },
  "IndicesSecundarios": [
    {
      "NombreIndice": "ClienteEstadoIndex",
      "ClaveParticion": "ClienteId",
      "ClaveOrdenamiento": "Estado"
    },
    {
      "NombreIndice": "FondoEstadoIndex",
      "ClaveParticion": "FondoId",
      "ClaveOrdenamiento": "Estado"
    }
  ]
}
```

*Tabla: Transacciones

```json
{
  "NombreTabla": "Transactions",
  "ClaveParticion": "Id",
  "ClaveOrdenamiento": "FechaCreacion",
  "Atributos": {
    "Id": "String (UUID)",
    "ClienteId": "String (UUID)",
    "FondoId": "String (UUID)",
    "SuscripcionId": "String (UUID)",
    "Tipo": "String (Suscripcion|Cancelacion|Dividendo)",
    "Monto": "Number",
    "Comisiones": "Number",
    "MontoNeto": "Number",
    "Estado": "String (Pendiente|Procesando|Completada|Fallida)",
    "MetodoPago": "String (TransferenciaBancaria|Tarjeta)",
    "FechaLiquidacion": "String",
    "FechaProceso": "String",
    "FechaCreacion": "String",
    "FechaActualizacion": "String",
    "NumeroReferencia": "String",
    "Canal": "String (Web|Movil|Sucursal)",
    "Descripcion": "String",
    "MotivoFalla": "String"
  },
  "IndicesSecundarios": [
    {
      "NombreIndice": "TransaccionesCliente",
      "ClaveParticion": "ClienteId",
      "ClaveOrdenamiento": "FechaCreacion"
    },
    {
      "NombreIndice": "TransaccionesFondo",
      "ClaveParticion": "FondoId",
      "ClaveOrdenamiento": "FechaCreacion"
    },
    {
      "NombreIndice": "EstadoProcesoIndex",
      "ClaveParticion": "Estado"
    }
  ]
}
```

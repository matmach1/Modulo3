const PENDING = 'Pendiente de clasificación'

export const STATUS = { Abierto: 'Abierto', EnProgreso: 'En progreso', Cerrado: 'Cerrado' }

export const TYPE = {
  Pendiente: PENDING,
  ActoInseguro: 'Acto inseguro',
  CondicionInsegura: 'Condición insegura',
  CasiAccidente: 'Casi accidente',
  RiesgoOperativo: 'Riesgo operativo',
  Otro: 'Otro',
}

export const CATEGORY = {
  Pendiente: PENDING,
  ProblemaInfraestructura: 'Problema de infraestructura',
  RiesgoElectrico: 'Riesgo eléctrico',
  UsoEpp: 'Uso de elementos de protección personal',
  Otro: 'Otro',
}

export const CRITICALITY = { Pendiente: PENDING, Baja: 'Baja', Media: 'Media', Alta: 'Alta', Critica: 'Crítica' }

export const label = (labels, value) => labels[value] ?? value

export const formatDate = (iso) => new Date(iso).toLocaleString('es-AR')

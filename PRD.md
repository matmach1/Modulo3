# PRD-001: Sistema Inteligente de Reporte y Prevención de Riesgos Laborales

## Contexto y Problema
En muchas empresas los empleados detectan situaciones peligrosas, pero no saben dónde reportarlas, no hay seguimiento, se pierden observaciones, no se priorizan los riesgos más críticos, no se tiene una base unificada y no se genera conocimiento para prevenir accidentes.

## Objetivos
Diseñar una aplicación web donde cualquier empleado habilitado pueda generar un reporte de: actos inseguros, condiciones inseguras, casi accidentes, riesgos operativos, problemas de infraestructura, etc. Se requiere que el reporte ingresado se clasifique automáticamente en un tipo y además en base a la información ingresada se calcule el nivel de criticidad.
Los reportes quedan por defecto en estado abierto y después tienen que ser tratados.

## Definiciones
- Roles de usuario: el sistema reconoce dos tipos de usuario, común y administrador. Los permisos específicos de cada rol quedan definidos por los RF que los referencian explícitamente (p. ej. RF-02/RF-03 para listado de reportes, RF-16 para cambio de estado) y se verifican mediante los AC de control de acceso (AC-03, AC-04, AC-12, AC-16).

## Requerimientos Funcionales
- RF-01: El sistema debe permitir que cualquier usuario autenticado cree un reporte indicando descripción, ubicación y, opcionalmente, fotos adjuntas y un tipo sugerido.
- RF-02: El sistema debe permitir que el administrador liste todos los reportes, paginados, indicando su estado (abierto, en progreso, cerrado).
- RF-03: El sistema debe permitir que el usuario común liste únicamente los reportes que él mismo creó.
- RF-04: El sistema debe clasificar automáticamente cada reporte, al momento de crearse, en una de las siguientes categorías: Problema de infraestructura, Riesgo eléctrico, Uso de elementos de protección personal u Otro, utilizando el modelo de clasificación configurado (Claude API). Estas categorías corresponden a la v1 y podrán ampliarse en futuras iteraciones.
- RF-05: El sistema debe conservar la clasificación automática (RF-04) como definitiva cuando la categoría sugerida por el usuario (RF-01) difiera de ella.
- RF-06: El sistema debe registrar ambas categorías, la sugerida por el usuario y la asignada automáticamente, para revisión del administrador cuando difieran entre sí.
- RF-07: El sistema debe asignar automáticamente a cada reporte una prioridad entre Baja, Media, Alta o Crítica, basándose en la descripción, ubicación y demás información ingresada, mediante el modelo de criticidad configurado.
- RF-08: El sistema debe registrar un evento de notificación cada vez que se crea un nuevo reporte. (El envío real de correo electrónico a administradores queda fuera de alcance en la v1; ver Fuera de Alcance.)
- RF-09: El sistema debe sugerir una acción preventiva predefinida según la criticidad del reporte, para los niveles Alta ("Requiere intervención inmediata") y Crítica ("Detener actividad"). La v1 no define una acción sugerida específica para los niveles Baja o Media.
- RF-10: El sistema debe permitir que el usuario creador edite un reporte mientras este permanezca en estado abierto.
- RF-11: El sistema debe impedir la modificación de un reporte una vez que el administrador lo cambie a En Progreso o Cerrado.
- RF-12: El sistema debe requerir autenticación (usuario + contraseña) para acceder a cualquier funcionalidad relacionada con reportes.
- (RF-13 removido: la existencia de los dos roles de usuario se documenta en la sección Definiciones, no como requerimiento funcional individual.)
- RF-14: El sistema debe permitir que un usuario se registre con email + contraseña.
- RF-15: El sistema debe permitir que un usuario modifique su contraseña.
- RF-16: El sistema debe permitir que el administrador cambie el estado de un reporte entre abierto, en progreso y cerrado.
- RF-17: El sistema debe registrar fecha, hora y usuario de creación de cada reporte.
- RF-18: El sistema debe registrar fecha, hora y usuario de cierre de cada reporte.
- RF-19: El sistema debe permitir que el usuario adjunte hasta 5 imágenes en formato JPG, JPEG o PNG, con un tamaño máximo de 2 MB por archivo.
- RF-20: El sistema debe almacenar las imágenes adjuntas en el repositorio documental de la aplicación, asociadas al reporte correspondiente.
- RF-21: En la v1, el sistema no debe utilizar las imágenes como input del modelo de clasificación; deben servir únicamente como evidencia visual para los administradores.
- RF-22: El sistema debe permitir que el administrador registre una acción correctiva o preventiva asociada a un reporte, indicando descripción, fecha y usuario responsable del registro.
- RF-23: Al cerrar un reporte, el sistema debe exigir que se indique la acción realizada y una observación de cierre.
- RF-24: El sistema debe mostrarle al administrador, en el panel principal, la cantidad de reportes con criticidad Crítica que se encuentren abiertos.

## Requerimientos No Funcionales
- RNF-01: La clasificación debe responder en < 3 s (p95).
- RNF-02: La clasificación debe obtener al menos 90% de aciertos sobre el conjunto de casos de prueba de clasificación, definido y validado por el área de Seguridad e Higiene (dataset de validación a versionar como entregable del proyecto; ver Riesgos y Dependencias).
- RNF-03: La asignación de criticidad debe obtener al menos 85% de aciertos sobre el conjunto de casos de prueba de criticidad, definido y validado por el área de Seguridad e Higiene (dataset de validación a versionar como entregable del proyecto; ver Riesgos y Dependencias).
- RNF-04: La API key del modelo no debe estar en el código; se lee de la variable de entorno API_KEY.
- RNF-05: Las contraseñas deben almacenarse con hash seguro (bcrypt/argon2), nunca en texto plano; la sesión expira tras 24 h de inactividad.

## Criterios de Aceptación
- AC-01 (RF-01): Dada una descripción vacía, cuando se intenta crear el reporte, entonces el sistema responde HTTP 400 y no lo crea.
- AC-02 (RF-02): Dado un conjunto de reportes existentes y los parámetros page y size provistos por el cliente, cuando el administrador los lista, entonces el sistema devuelve como máximo size reportes correspondientes a la página page solicitada.
- AC-03 (RF-02): Dado un usuario común autenticado, cuando intenta acceder al endpoint de listado global de reportes, entonces el sistema responde HTTP 403.
- AC-04 (RF-03): Dado el usuario A dueño de un reporte y el usuario B autenticado, cuando B intenta ver ese reporte, entonces el sistema responde HTTP 403 y no lo muestra. (control de acceso — OWASP #1)
- AC-05 (RF-04, RF-07, RNF-02, RNF-03): Dado el dataset de validación versionado (ver RNF-02, RNF-03 y Riesgos y Dependencias), cuando se ejecuta la clasificación automática de categoría y prioridad sobre el conjunto completo del dataset, entonces el porcentaje de aciertos de categoría es ≥ 90% y el de prioridad es ≥ 85%.
- AC-06 (RF-05, RF-06): Dado un reporte cuya categoría sugerida por el usuario difiere de la clasificación automática, cuando se crea el reporte, entonces el sistema conserva la clasificación automática como definitiva y registra ambas categorías (sugerida y automática) para revisión del administrador.
- AC-07 (RF-08): Dado un reporte recién creado, cuando finaliza el proceso de creación, entonces el sistema registra un evento de notificación asociado a ese reporte.
- AC-08 (RF-09): Dado un reporte clasificado con criticidad Crítica, cuando se visualiza el reporte, entonces el sistema muestra la recomendación "Detener actividad".
- AC-09 (RF-09): Dado un reporte clasificado con criticidad Alta, cuando se visualiza el reporte, entonces el sistema muestra la recomendación "Requiere intervención inmediata".
- AC-10 (RF-10): Dado un reporte en estado abierto cuyo propietario es el usuario autenticado, cuando modifica la descripción y guarda los cambios, entonces el sistema guarda la descripción modificada y la refleja en consultas posteriores.
- AC-11 (RF-11): Dado un reporte en estado En Progreso o Cerrado, cuando el usuario creador intenta editarlo, entonces el sistema rechaza la modificación.
- AC-12 (RF-12): Dado un usuario no autenticado, cuando intenta ver la lista de reportes, entonces el sistema responde HTTP 401 y no muestra ningún dato.
- AC-13 (RF-14): Dado un email no registrado previamente y una contraseña, cuando un usuario se registra, entonces el sistema crea la cuenta y permite autenticarse con esas credenciales.
- AC-14 (RF-15): Dado un usuario autenticado, cuando cambia su contraseña, entonces el sistema actualiza el hash almacenado, de modo que un inicio de sesión posterior solo es exitoso con la nueva contraseña.
- AC-15 (RF-16): Dado un administrador autenticado, cuando cambia el estado de un reporte de Abierto a En Progreso, entonces el nuevo estado queda registrado y visible en futuras consultas.
- AC-16 (RF-16): Dado un usuario común autenticado, cuando intenta cambiar el estado de un reporte, entonces el sistema responde HTTP 403.
- AC-17 (RF-17): Dado un reporte recién creado, cuando se consulta el detalle, entonces se muestran la fecha, hora y usuario de creación.
- AC-18 (RF-18): Dado un reporte cerrado por un administrador, cuando se consulta el detalle del reporte, entonces se muestran fecha, hora y usuario que realizó el cierre.
- AC-19 (RF-19): Dado un reporte con 5 imágenes ya adjuntas, cuando el usuario intenta adjuntar una sexta imagen, entonces el sistema rechaza el archivo y mantiene el límite de 5 imágenes.
- AC-20 (RF-19): Dado un archivo con formato distinto de JPG, JPEG o PNG, cuando el usuario intenta adjuntarlo a un reporte, entonces el sistema lo rechaza.
- AC-21 (RF-19): Dado un archivo con tamaño mayor a 2 MB, cuando el usuario intenta adjuntarlo a un reporte, entonces el sistema lo rechaza.
- AC-22 (RF-20): Dado un reporte con imágenes adjuntas, cuando se consulta su detalle, entonces las imágenes almacenadas se muestran asociadas a ese reporte.
- AC-23 (RF-21): Dados dos reportes con la misma descripción, ubicación y demás datos de texto, uno sin imágenes adjuntas y otro con imágenes adjuntas, cuando ambos se clasifican automáticamente, entonces el sistema asigna a ambos la misma categoría y prioridad.
- AC-24 (RF-22): Dado un administrador autenticado, cuando registra una acción correctiva sobre un reporte, entonces el sistema almacena la acción y la muestra en futuras consultas.
- AC-25 (RF-23): Dado un reporte en progreso, cuando el administrador lo cierra, entonces el sistema exige que se registre una acción correctiva y una observación de resolución antes de completar el cierre.
- AC-26 (RF-24): Dados N reportes con criticidad Crítica en estado abierto, cuando el administrador visualiza el panel principal, entonces el sistema muestra la cantidad N de esos reportes.

## Fuera de Alcance
CRM completo · multi-canal real (mail/WhatsApp en vivo) · RBAC configurable / más de dos roles · multi-tenant · envío real de mails (el borrador queda para copiar/pegar).
(La autenticación con dos roles (usuario común y administrador) sí forma parte del alcance — ver Definiciones y RF-12.)

## Riesgos y Dependencias
- Riesgo: clasificación incorrecta o inconsistente para descripciones ambiguas → mitigación: validación con dataset etiquetado y revisión periódica de casos erróneos.
- Dependencia: API de Claude · base de conocimiento kb.md · SQLite.

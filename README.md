# Read Me

## Nicolás Alejandro Romero Emparanza

### Ejecución

El proyecto fue desarrollado en Unity Engine en la versión 2022.3.15f1. Se recomienda usar esta misma versión para evitar posibles problemas de compatibilidad. [Link descarga: Unity 2022.3.15](https://unity3d.com/get-unity/download/archive).

Al abrir el proyecto con Unity, este abrirá en modo default. Para poder acceder al funcionamiento, se debe dar doble clic a la escena “SampleScene” que se encuentra en la ruta `Assets > Scene`.

Al abrir esta escena, podrás ver la jerarquía de la escena con una serie de objetos. El objeto llamado **ACO Experiment** contiene el código que permite ejecutar la solución. Además del código del experimento, se listan los grafos descritos en el informe.

En el área central, podrás ver gráficamente cómo estos grafos se encuentran distribuidos. Puedes hacer doble clic sobre los nombres de los esquemas para que la cámara de la visualización se centre en estos.

A continuación, se explican los componentes que permiten controlar la solución:

- **GraphTest**: Es una referencia a uno de los esquemas en escena.
- **Iteration**: Corresponde al número de iteraciones que se ejecutará la solución.
- **Ants Per Iteration**: Número de hormigas que se utilizan por iteración.
- **Pheromone Intensity**: Multiplicador de intensidad de la feromona dejada por las hormigas.
- **Evaporation Rate**: Multiplicador de disipación de valor de feromonas.
- **Seed**: Semilla controladora de valores aleatorios.
- **Evaluator Weight**: Pesos para las diferentes evaluaciones.
  - Evaluación de espacios vacíos
  - Evaluación de muro exterior
  - Evaluación de número de esquinas
- **Enforce Graph Structure**: Permite decidir si el mapa a construir debe respetar la forma del esquema dado.
- **Run Test Selected**: Ejecuta la solución usando el mapa seleccionado en “Graph Test”.
- **Run All**: Ejecuta la solución una vez para cada uno de los mapas que se encuentran en las escenas.

### Código y Salidas

Todos los archivos de código se encuentran en la ruta `Assets > Nicolas-Romero`.

Al momento de ejecutarse la solución, se creará una carpeta `OutputExperiment`. Dicha carpeta contendrá una subcarpeta de la solución generada y la data a extraer para el análisis.

**OJO:** Al ser ejecutada nuevamente, la solución elimina la carpeta `OutputExperiment` y todo su contenido. Si se requiere volver a ejecutar la solución, se recomienda mover el contenido que se desea guardar de esta ruta para evitar que esta sea eliminada por error.

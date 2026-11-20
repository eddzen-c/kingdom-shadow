# Kingdom Shadow - 600 metros y seis sectores por escenario

Cada escenario debe medir **600 metros** con **seis sectores consecutivos de 100 metros**. La escala conserva la referencia oficial de 1 unidad de Unity = 1 metro.

## Escenas ya existentes (no se modifican en esta rama)
- `Assets/Game/Scenes/Levels/PuebloLunaria_600m.unity`
- `Assets/Game/Scenes/Levels/BosqueEncantado_600m.unity`

## Escenarios nuevos que crea el generador
- `Assets/Game/Scenes/Levels/MontanasHeladas_600m.unity`
- `Assets/Game/Scenes/Levels/CastilloSombras_600m.unity`

Abre Unity 6000.6.0f1 y utiliza:
- `Tools > Kingdom Shadow > Build Montañas Heladas 600m (6 sectores)`
- `Tools > Kingdom Shadow > Build Castillo de las Sombras 600m (6 sectores)`

Los dos nuevos mapas adaptan los planos exactos del paquete `Planos_exactos_Unity_2_5D_Lunaria(2).zip`. Los trazados originales eran de **144 m y 176 m**, respectivamente. Para llegar a 600 m se diseñaron **sectores adicionales**: no se cambiaron silenciosamente los documentos de referencia.

## Montañas Heladas (0–600 m)
| Sector | Recorrido | Uso |
| --- | --- | --- |
| 01 | 0–100 m | Sendero de nieve, dos rampas de hielo |
| 02 | 100–200 m | Cornisas, salto de 4 m y carámbanos |
| 03 | 200–300 m | Descenso, abismo de 4 m, ascenso |
| 04 | 300–400 m | Cumbres y pasos elevados |
| 05 | 400–500 m | Último abismo y ascenso helado |
| 06 | 500–600 m | Arena helada, marcador de jefe y cristal 03 |

## Castillo de las Sombras (0–600 m)
| Sector | Recorrido | Uso |
| --- | --- | --- |
| 01 | 0–100 m | Puerta, escalinata y primer puente |
| 02 | 100–200 m | Galería, patio y espinas |
| 03 | 200–300 m | Escalinata alta y segundo puente |
| 04 | 300–400 m | Torres, espinas y descenso |
| 05 | 400–500 m | Antesalas y aproximación a la arena |
| 06 | 500–600 m | Arena final, marcador Guardián y cristal 04 |

## Integración y pruebas pendientes
- Los escenarios se generan al ejecutar las opciones del editor; esta rama **no contiene aún los archivos .unity generados**.
- Los marcadores de cristales, jefes y checkpoints son **puntos de referencia**, no sistemas funcionales.
- Los triggers de peligros aún no causan daño.
- Se reutilizan `PlayerBlockoutController` y `CameraFollow2D` sin modificarlos.
- Comprobar en Unity: compilación, continuidad entre seis sectores, salto, fricción de hielo, superficies, cámara, menú, colisiones y compatibilidad con las mecánicas de otros integrantes.
- Tras generar los niveles, versionar sus `.unity`, `.meta`, materiales y `.meta` antes del Pull Request.
- Conservar los CSV, PNG y SVG originales como fuente de diseño; no sustituirlos por estos planos ampliados.

README FISH SWARM

Das SwarmManager Prefab sollte in die Szene gesetzt werden. 

	- ein Prefab für die einzelnen Fische muss angegeben werden
	- ein Goal-Gameobject muss angegeben werden. Dadurch wird die Zielposition des Fisch Schwarms festgelegt

	- Swim Limits: innerhalb des angegebenen Bereichs um den SwarmManager werden die Fische zu Beginn zufällig verteilt
	- Num Fish: die Anzahl der Fische im Schwarm 
	- Obstacles: Hindernisse, bei welchen die Fische blockiert werden und nicht hindurch kommen können
	- Rejections: Hindernisse, die der Fischschwarm frühzeitig erkennt und umschwimmen kann
			- detection value: der Abstandswert ab welchem ein Hindernis erkannt wird
			- rejection min&max speed: Rotationsgeschwindigkeit zum wegdrehen. Ein Wert in diesem Bereich wird aufgrund 
				der Entfernung zum Hindernis berechnet 
	- Min & Max Speed: die Fische bekommen eine zufällige Geschwindigkeit in diesem Bereich 
	- Neighbour Distnace: die Fische berechnen Rotation und Richtung anhand ihrer Nachbarn. Dieser Wert bestimmt welche Fische aufgrund ihrer Distanz
		als Nachbarn in Frage kommen
	- Avoid Value: bestimmt wie viel Abstand die Fische zueinander einhalten 

! viele Werte sind voneinander abhängig. Außerdem wird das Jobs und Burst package benötigt.
	:) 
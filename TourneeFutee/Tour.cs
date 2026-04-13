namespace TourneeFutee
{
    // Modélise une tournée dans le cadre du problème du voyageur de commerce

    // NOTE : nécessite que Tour expose un constructeur Tour(List<string>, float)
    // et une propriété Vertices — à ajouter dans Tour.cs
    public class Tour
    {      
        List<(string source, string destination)> segment;
        float cost;
        List<string> vertices;

        public Tour(List<(string source, string destination)> segment, float cost)
        {
            this.segment = segment;
            this.cost = cost;
        }
        public Tour()
        {
            this.segment = new List<(string source, string destination)>();
            this.cost = 0.0f ;
        }

        //Constructeur pour l'objectif 3
        public Tour(List<string> vertices, float cost)
        {
            this.vertices =vertices;
            this.cost = cost;
        }
        // propriétés

        // Coût total de la tournée
        public float Cost
        {
            get { return this.cost; }   
        }

        // Nombre de trajets dans la tournée
        public int NbSegments
        {
            get { return this.segment.Count; }   
        }

        //Propriété pour l'objectif 3
        public List<string> Vertices
        {
            get { return this.vertices; }
        }
        // Renvoie vrai si la tournée contient le trajet `source`->`destination`
        public bool ContainsSegment((string source, string destination) segment)
        {
            return this.segment.Contains(segment); //Retourne vraie si la liste segment contient le segment, retourne faux sinon
        }


        // Affiche les informations sur la tournée : coût total et trajets
        public void Print()
        {
            Console.WriteLine($"Coût total de la tournée : {this.cost}");
            Console.WriteLine("Trajets :");
            foreach ((string source, string destination) segment in this.segment)
            {
                Console.WriteLine($"{segment.source} -> {segment.destination}");
            }
        }

        // TODO : ajouter toutes les méthodes que vous jugerez pertinentes 

    }
}

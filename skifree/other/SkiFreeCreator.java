import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.text.SimpleDateFormat;
import java.util.LinkedList;
import java.util.List;

public class SkiFreeCreator {
	// yes i hardcoded file locations, fuck you
	static File csv = new File("D:/Libraries/Documents/terrain list.csv");
	
	public static void main(String[] args) throws IOException {
		System.out.println("// SkiFree Terrain List");
		System.out.println("// Generation Date: " + new SimpleDateFormat("yyyy-MM-dd HH:mm:ss").format(csv.lastModified()));
		System.out.println();
		System.out.println("// A good terrain has the following qualities:");
		System.out.println("// - doesn't have a bunch of flat ground, even if it's right outside the mission bounds (high octane)"); 
		System.out.println("// - is not fucking gigantic (stripmine, a bunch of other TR2 terrains)");
		System.out.println("// - doesn't have a bunch of steep plateaus");
		System.out.println("// - is not Magnum (a map where the fastest route is to discjump off a bunch of flat ridges)");
		System.out.println("// use $TerrainTest to test a terrain locally");
		System.out.println();
		System.out.println("%i = -1; // %i++ is pre-increment for some reason; it's -1 so it can start at 0");
		System.out.println();
		
		List<String> fileLines = Files.readAllLines(csv.toPath());
		List<Terrain> terrainList = new LinkedList<>();
		
		for( String line : fileLines ) {
			String[] split = line.split(",");
			if( split.length == 0 ) continue;
			if( !split[0].endsWith(".ter") ) continue;
			terrainList.add(new Terrain(split));
		}
		
		// if you don't want me at my new String[][] {}, you don't deserve me at my ______________
		for( String[] output : new String[][] {
			{"ACCEPTED TERRAINS", ""},
			{"REJECTED FOR DEADSTOPS", "DEADSTOP"},
			{"REJECTED FOR BEING UNSKIIABLE", "VARIANCE"},
			{"REJECTED FOR SOME OTHER REASON", "OVERRIDE"},
			{"DUPLICATES", "DUPLICATE"}
		}) {
			String listName = output[0];
			String rejectReason = output[1];
			
			// count up how many there are
			int count = 0;
			for( Terrain ter : terrainList ) {
				if( rejectReason.equals(ter.rejectReason) ) {
					count++;
				}
			}
			
			System.out.println("// " + listName + " (" + count + ")");
			for( Terrain ter : terrainList ) {
				if( rejectReason.equals(ter.rejectReason) ) {
					System.out.println(ter);
				}
			}
			
			System.out.println();
		}
		System.out.println("$SkiFreeTerrainListMAX = %i;");
	}
	
	static class Terrain {
		String terrainName;
		String result;
		String rejectReason;
		String comment;
		
		public Terrain(String[] split) {
			terrainName = split[0];
			result = split[4];
			
			if( "Yes".equals(split[1]) ) {
				rejectReason = "DEADSTOP";
			}
			else if( "Yes".equals(split[2]) ) {
				rejectReason = "VARIANCE";
			}
			else if( "Yes".equals(split[3]) ) {
				rejectReason = "OVERRIDE";
			}
			else if ( "Duplicate".equals(result) ) {
				rejectReason = "DUPLICATE";
			}
			else {
				rejectReason = "";
			}
			
			if( split.length > 5 ) {
				comment = split[5];
				if( comment.startsWith("\"") ) {
					comment = comment.substring(1, comment.length() - 1);
				}
			}
			else {
				comment = "";
			}
		}
		
		@Override
		public String toString() {
			// automatically makes string builder on real versions of java, fuck you
			return
				("Accept".equals(result) ? "" : "//")
				+ "$SkiFreeTerrainList[%i++] = \""
				+ terrainName
				+ "\";"
				+ (!comment.isEmpty() ? (" // " + comment) : "")
			;
		}
	}
}

<?php

function render_elvenar_data($path_to_xml_file){
		if (!file_exists($path_to_xml_file)){
			return;
		}else{
			$chars_to_replace = array('[\r]','[\n]','[\t]');
			$xmlstring = trim(preg_replace($chars_to_replace, '', file_get_contents($path_to_xml_file)));
		}
		echo '<table border="1">'."\n";
		echo '<tr><th>Id</th><th>Name</th><th>Level</th><th>Race</th><th>Size</th></tr>'."\n";
		$xml = new SimpleXMLElement($xmlstring);
		foreach ($xml->CityEntity as $record) {
			echo '<tr>'."\n";			
			echo '<td>'.$record->Id.'</td>'."\n";
			echo '<td>'.$record->Name.'</td>'."\n";
			echo '<td>'.$record->Level.'</td>'."\n";
			echo '<td>'.$record->Race.'</td>'."\n";
			echo '<td>'.$record->Size['X'].' x '.$record->Size['Y'].'</td>'."\n";
			echo '</tr>'."\n";
		}
		echo '</table>'."\n";
	}
?>
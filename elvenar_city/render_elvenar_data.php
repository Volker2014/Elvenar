<?php

function render_elvenar_data($path_to_file){
        if (!file_exists($path_to_file)){
            return;
        }else{
            $chars_to_replace = array('[\r]','[\n]','[\t]');
            $xmlstring = trim(preg_replace($chars_to_replace, '', file_get_contents($path_to_file)));
        }
        
        $hasTitle = true; 

        echo '<table border="1">';
        
        $handle = fopen($path_to_file, "r"); 
        $start = 0; 

        while (($data = fgetcsv($handle, 1000, ";")) !== FALSE)  
        { 
          echo '<tr>' . "\n"; 
   
          for ( $x = 0; $x < count($data); $x++) 
          { 
            if ($start == 0 && $hasTitle == true) 
                echo '<th>'.$data[$x].'</th>' . "\n"; 
            else 
                echo '<td>'.$data[$x].'</td>' . "\n"; 
          } 
     
          $start++; 
     
          echo '</tr>' . "\n";    
        } 

        fclose($handle);
        echo '</table>'."\n";
    }
?>
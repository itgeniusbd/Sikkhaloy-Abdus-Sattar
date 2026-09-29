<?php

function safa_web_base()
{
  $script = str_replace('\\', '/', $_SERVER['SCRIPT_NAME'] ?? '');
  $dir = dirname($script);
  $leaf = basename($dir);
  if ($leaf === 'install' || $leaf === 'api') {
    $dir = dirname($dir);
  }
  if ($dir === '/' || $dir === '.' || $dir === '\\') {
    return '';
  }
  return rtrim($dir, '/');
}
